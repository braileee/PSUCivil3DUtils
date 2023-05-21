using Autodesk.Civil.DatabaseServices;
using Civil3DSubassemblyAnnotations.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DSubassemblyAnnotations.Events
{
    public class CorridorCodeUpdateEvent : PubSubEvent<CorridorCode>
    {
    }
}
