using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsystBussiness.Core
{
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        // boiler-plate
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler eventHandler = this.PropertyChanged;
            try
            {
                if (eventHandler != null)
                {
                    eventHandler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.Catch(ex);
            }
        }

        protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {

            try
            {
                MemberExpression memberExpression = propertyExpression?.Body as MemberExpression;
                if (!string.IsNullOrEmpty(memberExpression?.Member?.Name))
                {
                    OnPropertyChanged(memberExpression.Member.Name);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.Catch(ex);
            }
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            try
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
            catch (Exception ex)
            {
                ExceptionHandler.Catch(ex);
            }
            return true;
        }
    }
}
