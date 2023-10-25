using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DExtractCorridorLinksToSurfaces.Models
{
    public class OverhangCorrectionWrapper
    {
        public OverhangCorrectionType OverhangCorrectionType { get; set; }
        public string Name
        {
            get
            {
                return Enum.GetName(typeof(OverhangCorrectionType), OverhangCorrectionType);
            }
        }

        public static List<OverhangCorrectionWrapper> Get()
        {
            List<string> names = Enum.GetNames(typeof(OverhangCorrectionType)).ToList();

            List<OverhangCorrectionWrapper> wrappers = new List<OverhangCorrectionWrapper>();

            foreach (string name in names)
            {
                Enum.TryParse(name, out OverhangCorrectionType type);
                wrappers.Add(new OverhangCorrectionWrapper { OverhangCorrectionType = type });
            }

            return wrappers;
        }
    }
}
