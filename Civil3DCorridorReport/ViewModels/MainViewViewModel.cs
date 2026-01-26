using AutoCADUtils.Utils;
using Autodesk.AutoCAD.ApplicationServices;
using AcadDb = Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.Settings;
using Civil3DCorridorReport.Models;
using Civil3DCorridorReport.Models.Json;
using Civil3DUtils.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace Civil3DCorridorReport.ViewModels
{
    public class MainViewViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;

        public Settings Settings { get; }
        public List<Corridor> Corridors { get; } = new List<Corridor>();
        public DelegateCommand GenerateReportCommand { get; }

        private Corridor selectedCorridor;

        public Corridor SelectedCorridor
        {
            get { return selectedCorridor; }
            set
            {
                selectedCorridor = value;
                RaisePropertyChanged();
            }
        }


        public MainViewViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;


            DocumentCollection documentCollection = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            documentCollection.DocumentActivated -= DocumentCollectionDocumentActivated;
            documentCollection.DocumentActivated += DocumentCollectionDocumentActivated;

            Settings = SettingsLoader.Load();

            Corridors = CorridorUtils.GetAllTheCorridors(AcadDb.OpenMode.ForRead);

            GenerateReportCommand = new DelegateCommand(OnGenerateReportCommand);
        }

        private void OnGenerateReportCommand()
        {
            try
            {
                CorridorData corridorData = new CorridorData(SelectedCorridor, Settings);
                List<CorridorDataItem> corridorDataItems = corridorData.Get();

                Point3d? point = PromptUtils.PromptPoint();

                if (!point.HasValue)
                {
                    return;
                }

                CreateTable(corridorDataItems, point.Value);
            }
            catch (Exception)
            {
                MessageBoxUtils.ShowError("An error occurred while generating the report.");
            }
        }

        private void CreateTable(List<CorridorDataItem> data, Point3d insertionPoint)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            AcadDb.Database db = doc.Database;

            using (DocumentUtils.LockActiveDocument())
            {
                using (AcadDb.Transaction tr = db.TransactionManager.StartTransaction())
                {
                    AcadDb.BlockTable bt = (AcadDb.BlockTable)tr.GetObject(db.BlockTableId, AcadDb.OpenMode.ForRead);
                    AcadDb.BlockTableRecord btr = (AcadDb.BlockTableRecord)tr.GetObject(bt[AcadDb.BlockTableRecord.ModelSpace], AcadDb.OpenMode.ForWrite);

                    // Create table
                    AcadDb.Table table = new AcadDb.Table
                    {
                        TableStyle = db.Tablestyle,
                        Position = insertionPoint
                    };

                    int rows = data.Count + 1; // header + data rows
                    int cols = 6;

                    table.SetSize(rows, cols);
                    table.SetRowHeight(8);
                    table.SetColumnWidth(25);

                    // Header row
                    table.Cells[0, 0].TextString = "St.";
                    table.Cells[0, 1].TextString = "Konstruktiontype";
                    table.Cells[0, 2].TextString = "Topkote";
                    table.Cells[0, 3].TextString = "Slope Front";
                    table.Cells[0, 4].TextString = "Slope Back";
                    table.Cells[0, 5].TextString = "Bredde krone [m]";

                    // Fill rows
                    for (int i = 0; i < data.Count; i++)
                    {
                        var item = data[i];
                        int r = i + 1;

                        table.Cells[r, 0].TextString = $"{item.StartStationFormatted} - {item.EndStationFormatted}";
                        table.Cells[r, 1].TextString = item.AssemblyName;
                        table.Cells[r, 2].TextString = item.TopElevation.ToString("+0.0;-0.0;0.0");
                        table.Cells[r, 3].TextString = item.SlopeBackFormatted;
                        table.Cells[r, 4].TextString = item.SlopeBackFormatted;
                        table.Cells[r, 5].TextString = item.Width.ToString("0.0");
                    }

                    table.GenerateLayout();
                    btr.AppendEntity(table);
                    tr.AddNewlyCreatedDBObject(table, true);

                    tr.Commit();
                }
            }
        }



        private void DocumentCollectionDocumentActivated(object sender, DocumentCollectionEventArgs e)
        {
            RaiseCloseRequest();
        }

        protected void RaiseCloseRequest()
        {
            OnRequestClose?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler OnRequestClose;
    }
}
