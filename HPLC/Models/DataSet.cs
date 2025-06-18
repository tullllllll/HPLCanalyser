using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Input;

namespace HPLC.Models;

public class DataSet : INotifyPropertyChanged
{
    [NotMapped] private DateTime _dateAdded;

    [NotMapped] private DateTime _lastUsed;

    [NotMapped] private string _name;

    [NotMapped] private DateTime _sampleDate;

    [Key] public int ID { get; set; }

    [Required]
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    [Required]
    public DateTime Date_Added
    {
        get => _dateAdded;
        set
        {
            _dateAdded = value;
            OnPropertyChanged(nameof(Date_Added));
        }
    }

    [Required]
    public DateTime Last_Used
    {
        get => _lastUsed;
        set
        {
            _lastUsed = value;
            OnPropertyChanged(nameof(Last_Used));
        }
    }

    public DateTime Sample_Date
    {
        get => _sampleDate;
        set
        {
            _sampleDate = value;
            OnPropertyChanged(nameof(Sample_Date));
        }
    }

    public ICollection<DataPoint> DataPoints { get; set; } = [];

    [NotMapped] public ICommand? SelectCommand { get; set; }

    [NotMapped] public ICommand? DeleteCommand { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}