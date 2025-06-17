using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Input;

namespace HPLC.Models
{
    public class DataSet : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        [Key]
        public int ID { get; set; }

        [NotMapped]
        private string _name;
        [Required]
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        [NotMapped]
        private DateTime _dateAdded;
        [Required]
        public DateTime Date_Added
        {
            get => _dateAdded;
            set { _dateAdded = value; OnPropertyChanged(nameof(Date_Added)); }
        }
        [NotMapped]
        private DateTime _lastUsed;
        [Required]
        public DateTime Last_Used
        {
            get => _lastUsed;
            set { _lastUsed = value; OnPropertyChanged(nameof(Last_Used)); }
        }
        [NotMapped]
        private DateTime _sampleDate;
        public DateTime Sample_Date
        {
            get => _sampleDate;
            set { _sampleDate = value; OnPropertyChanged(nameof(Sample_Date)); }
        }

        public ICollection<DataPoint> DataPoints { get; set; } = [];

        [NotMapped]
        public ICommand? SelectCommand { get; set; }
        [NotMapped]
        public ICommand? DeleteCommand { get; set; }
    }
}