using Autodesk.Civil.DatabaseServices;

namespace Civil3DBatchExtractSolids.Models
{
    public class CorridorWrapper
    {
        private CorridorWrapper()
        {
        }

        public Corridor Model { get; set; }

        public string Name
        {
            get
            {
                return Model.Name;
            }
        }

        public static CorridorWrapper Create(Corridor corridor)
        {
            return new CorridorWrapper
            {
                Model = corridor
            };
        }
    }
}
