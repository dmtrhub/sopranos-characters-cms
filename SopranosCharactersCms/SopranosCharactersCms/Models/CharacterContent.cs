using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace SopranosCharactersCms.Models
{
    public class CharacterContent : INotifyPropertyChanged
    {
        private bool _isSelected;

        public int Id { get; set; }

        public string FullName { get; set; }

        public string Role { get; set; }

        public string ImagePath { get; set; }

        public string RtfPath { get; set; }

        public string DateAddedUtc { get; set; }

        [XmlIgnore]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public string DateAddedDisplay
        {
            get
            {
                if (DateTime.TryParse(DateAddedUtc, null, DateTimeStyles.RoundtripKind, out DateTime dateAdded))
                {
                    return dateAdded.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                }

                return DateAddedUtc;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
