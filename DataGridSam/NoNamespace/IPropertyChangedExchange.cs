using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridSam;

public interface INotifyPropertyChangedFast : INotifyPropertyChanged
{
    object? GetPropertyValue(string path);
}