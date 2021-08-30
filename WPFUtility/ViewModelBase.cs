using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Windows;

namespace Wildgoat.WPFUtility
{
    /// <summary>
    /// Delegate used for the properties callback
    /// </summary>
    /// <param name="sender">view model object</param>
    /// <param name="e">args of the callback</param>
    public delegate void PropertyCallback(object sender, CallbackEventArgs e);

    /// <summary>
    /// Delegate used to either save an object or revert this object to the previous state
    /// </summary>
    /// <param name="sender">view model object</param>
    /// <param name="value">value to save or load to/form the data holder</param>
    /// <param name="holder">data holder that parsists through calls</param>
    public delegate void PropertyEdition(object sender, object value, DataHolder holder);

    /// <summary>
    /// Delegate used for data validation
    /// </summary>
    /// <param name="value">value to validate</param>
    /// <returns>A list of errors. Null or empty if no errors</returns>
    public delegate IEnumerable<IError>? PropertyValidation(object value);

    /// <summary>
    /// Base interface for an error
    /// </summary>
    public interface IError : IEquatable<IError?>
    {
        /// <summary>
        /// Unique ID of the error, used for errors change detection
        /// </summary>
        int Code { get; }
    }

    /// <summary>
    /// Helpful class to manage the save/loading of a property edition
    /// </summary>
    public static class CollectionEditionDelegate
    {
        /// <summary>
        /// Load method to fill a value with the holder's data. Should only be passed to a PropertyDetails constructor
        /// </summary>
        /// <typeparam name="T">type of the objects contained in the collection</typeparam>
        /// <param name="sender"></param>
        /// <param name="value"></param>
        /// <param name="holder"></param>
        public static void RevertEditing<T>(object sender, object value, DataHolder holder)
        {
            var collection = (ICollection<T>)value;
            collection.Clear();
            foreach (var item in (IEnumerable<T>)holder.Data)
                collection.Add(item);
        }

        /// <summary>
        /// Save method to fill the holder's data from a collection. Should only be passed to a PropertyDetails constructor
        /// </summary>
        /// <typeparam name="T">type of the objects contained in the collection</typeparam>
        /// <param name="sender"></param>
        /// <param name="value"></param>
        /// <param name="holder"></param>
        public static void StartEditing<T>(object sender, object value, DataHolder holder)
        {
            holder.Data = ((ICollection<T>)value).ToArray();
        }
    }

    /// <summary>
    /// Arguments of the callback method
    /// </summary>
    public class CallbackEventArgs : ValueChangedEventArgs<object>
    {
#pragma warning disable CS8604
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="oldValue">Previous value, before it is changed</param>
        /// <param name="newValue">value, after the object is changed</param>

        public CallbackEventArgs(object? oldValue, object? newValue) : base(oldValue, newValue)
        { }

#pragma warning restore CS8604
    }

    /// <summary>
    /// Simple placeholder for any kind of data that persists when starting an edition or canceling
    /// </summary>
    public class DataHolder
    {
#pragma warning disable CS8625

        /// <summary>
        /// Data held
        /// </summary>
        public object Data { get; set; } = null;

#pragma warning restore CS8625
    }

    /// <summary>
    /// Base implementation of an error
    /// </summary>
    public class Error : IError
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="code">Unique ID of the error</param>
        /// <param name="message">Description of the error</param>
        public Error(int code, string message) => (Code, Message) = (code, message);

        public int Code { get; }

        /// <summary>
        /// Message of the error
        /// </summary>
        public string Message { get; }

        public bool Equals(IError? other) => other != null
                                                ? Code == other.Code
                                                : false;

        public override bool Equals(object? obj) => obj is IError other
                                                        ? Equals(other)
                                                        : false;

        public override int GetHashCode()
        {
            return HashCode.Combine(Code);
        }

        public override string ToString() => Message;
    }

    /// <summary>
    /// Defines custom information to use for certain properties
    /// </summary>
    public class PropertyDetails
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defaultValue">Value given to the property if none is set</param>
        /// <param name="callback">Callback method when changing the value of the property</param>
        /// <param name="validationRule">Method that define rule checks for the property value</param>
        /// <param name="startEditing">Method called when a edition action is done on this property</param>
        /// <param name="revertEditing">Method called when a revert action is done on this property</param>
        public PropertyDetails(
            object? defaultValue = null,
            PropertyCallback? callback = null,
            PropertyValidation? validationRule = null,
            PropertyEdition? startEditing = null,
            PropertyEdition? revertEditing = null)
            => (DefaultValue, Callback, ValidationRule, StartEditing, RevertEditing) = (defaultValue, callback, validationRule, startEditing, revertEditing);

        /// <summary>
        /// Callback method when changing the value of the property
        /// </summary>
        public PropertyCallback? Callback { get; }

        /// <summary>
        /// Value given to the property if none is set
        /// </summary>
        public object? DefaultValue { get; }

        /// <summary>
        /// Method called when a revert action is done on this property
        /// </summary>
        public PropertyEdition? RevertEditing { get; }

        /// <summary>
        /// Method called when a edition action is done on this property
        /// </summary>
        public PropertyEdition? StartEditing { get; }

        /// <summary>
        /// Method that define rule checks for the property value
        /// </summary>
        public PropertyValidation? ValidationRule { get; }
    }

    /// <summary>
    /// Base class for advanced view model management
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, INotifyPropertyChanging, IEditableObject, INotifyDataErrorInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        protected ViewModelBase()
        {
            Properties = new Dictionary<string, object?>();
            Errors = new Dictionary<string, IEnumerable<IError>?>();
            HoldSpecialData = new Dictionary<string, DataHolder>();
            BackupProperties = null;
        }

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        public event PropertyChangingEventHandler? PropertyChanging;

        public bool HasErrors
        {
            get
            {
                foreach (var error in Errors)
                    if (error.Value != null && error.Value.Any())
                        return true;
                return false;
            }
        }

        /// <summary>
        /// Map of all the property details to use. Key is the name of the property, value is the details
        /// </summary>
        protected abstract Dictionary<string, PropertyDetails>? SpecialProperties { get; }

        private Dictionary<string, object?>? BackupProperties { get; set; }
        private Dictionary<string, IEnumerable<IError>?> Errors { get; }
        private Dictionary<string, DataHolder> HoldSpecialData { get; }
        private Dictionary<string, object?> Properties { get; set; }

        public void BeginEdit()
        {
            if (BackupProperties == null)
            {
                BackupProperties = Properties.ToDictionary(entry => entry.Key, entry => entry.Value);
                if (SpecialProperties != null)
                    foreach (var property in SpecialProperties)
                        if (property.Value.StartEditing != null)
                        {
                            var holder = HoldSpecialData[property.Key] = new DataHolder();
                            property.Value.StartEditing(this, GetValue(property.Key), holder);
                        }
            }
        }

        public void CancelEdit()
        {
            if (BackupProperties != null)
            {
                RollBack(BackupProperties);
                BackupProperties = null;
            }
        }

        public void EndEdit()
        {
            BackupProperties = null;
        }

        public IEnumerable GetErrors(string propertyName) => Errors[propertyName] ?? Array.Empty<IError>();

#pragma warning disable CS8603

        /// <summary>
        /// Returns the value of a property.
        /// </summary>
        /// <param name="property">Name of the property</param>
        /// <returns>The value of the property</returns>
        protected object GetValue(string property) => Properties.ContainsKey(property)
                                                        ? Properties[property]
                                                        : SpecialProperties != null && SpecialProperties.ContainsKey(property)
                                                            ? SpecialProperties[property].DefaultValue
                                                            : null;

#pragma warning restore CS8603

        /// <summary>
        /// Changes the value of a property
        /// </summary>
        /// <param name="property">Name of the property</param>
        /// <param name="value">New value of the property</param>
        protected void SetValue(string property, object? value)
        {
            if (GetValue(property) != value)
            {
                if (SpecialProperties != null && SpecialProperties.ContainsKey(property))
                    SpecialProperties[property].Callback?.Invoke(this, new CallbackEventArgs(GetValue(property), value));
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(property));
                Properties[property] = value;
                var oldHasErrors = HasErrors;
                ValueChanged(property, value);
                if (HasErrors != oldHasErrors)
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasErrors)));
            }
        }

        private void RollBack(Dictionary<string, object?> oldValues)
        {
            var oldHasErrors = HasErrors;
            var rejectedValues = Properties;
            Properties = oldValues;
            if (SpecialProperties != null)
                foreach (var property in SpecialProperties)
                    if (property.Value.RevertEditing != null)
                        property.Value.RevertEditing(this, GetValue(property.Key), HoldSpecialData[property.Key]);
            var changedValues = rejectedValues.Where(entry => Properties.ContainsKey(entry.Key)).ToDictionary(entry => entry.Key, entry => (entry.Value, Properties[entry.Key]));
            foreach (var entry in changedValues)
                if (entry.Value.Value != entry.Value.Item2)
                    ValueChanged(entry.Key, entry.Value.Item2);
            var removedValues = rejectedValues.Where(entry => !Properties.ContainsKey(entry.Key)).Select(entry => entry.Key);
            foreach (var property in removedValues)
                ValueChanged(property, null);
            var addedValues = Properties.Where(entry => !rejectedValues.ContainsKey(entry.Key)).ToDictionary(entry => entry.Key, entry => entry.Value);
            foreach (var entry in addedValues)
                ValueChanged(entry.Key, entry.Value);
            if (HasErrors != oldHasErrors)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasErrors)));
        }

        private void ValueChanged(string property, object? value)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            if (SpecialProperties != null && SpecialProperties.ContainsKey(property))
            {
                var currentErrors = Errors.ContainsKey(property)
                    ? Errors[property] ?? Array.Empty<IError>()
                    : Array.Empty<IError>();
#pragma warning disable CS8604
                var newErrors = SpecialProperties[property].ValidationRule?.Invoke(value) ?? Array.Empty<IError>();
#pragma warning restore CS8604
                if (!currentErrors.SequenceEqual(newErrors))
                {
                    Errors[property] = newErrors;
                    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(property));
                }
            }
        }
    }
}