using Civil3DBatchExtractSolids.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DBatchExtractSolids.Events
{
    public class ShapeSelectEvent : PubSubEvent<ShapeWrapper>
    {
    }
}
