using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evaluation_task_3
{
    internal interface IIntervalProvider
    {
        Interval GetIntervalFromCsv(string filePath);
    }
}
