using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace automationApp
{
    public class ApplicationStatus : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; }
        public string FIO { get; set; }

        private Color _backgroundColor;
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                OnPropertyChanged();
            }
        }

        private Color _borderColor;
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                OnPropertyChanged();
            }
        }

        private Color _fioTextColor;
        public Color FioTextColor
        {
            get => _fioTextColor;
            set
            {
                _fioTextColor = value;
                OnPropertyChanged();
            }
        }

        private Color _dateTextColor;
        public Color DateTextColor
        {
            get => _dateTextColor;
            set
            {
                _dateTextColor = value;
                OnPropertyChanged();
            }
        }

        private Color _statusTextColor;
        public Color StatusTextColor
        {
            get => _statusTextColor;
            set
            {
                _statusTextColor = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}