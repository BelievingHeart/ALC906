using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Core.ViewModels.Application;
using MaterialDesignThemes.Wpf;
using WPFCommon.Commands;
using WPFCommon.Helpers;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.TimeLine
{
    public class TimeLineManagerViewModel : ViewModelBase
    {
        private string _serializationDir;

        #region prop

        public double DaysExpired { get; set; } = 2;

        public ICommand DeleteSingleTimeLineCommand { get; set; }
        public ObservableCollection<TimeLineItemViewModel> TimeLineItems { get; set; }

        public bool ShouldDialogOpen { get; set; }
        

        #endregion

        #region ctor

        public TimeLineManagerViewModel(string timeLineItemsSerializationDir)
        {
            _serializationDir = timeLineItemsSerializationDir;
            TimeLineItems = new ObservableCollection<TimeLineItemViewModel>(AutoSerializableHelper
                .LoadAllAutoSerializables<TimeLineItemViewModel>(timeLineItemsSerializationDir));
            
            DeleteSingleTimeLineCommand = new ParameterizedCommand(DeleteSingleTimeLine);
        }

        public void AddTimeLineItem(string comment)
        {
            var newItem = new TimeLineItemViewModel()
            {
                Comment = comment, ProductType = ApplicationViewModel.Instance.CurrentProductType,
                StartTime = DateTime.Now
            };
            Add(newItem);
            // Delete outdated timelines 
            DeleteOutdatedItems();

            ShouldDialogOpen = false;
        }

        private void DeleteSingleTimeLine(object obj)
        {
            var chip = (Chip) obj;
            var item = (TimeLineItemViewModel) chip.DataContext;
            Delete(item);
        }



        #endregion

        #region api

        private void Add(TimeLineItemViewModel timeLineItem)
        {
            // Add to list
            TimeLineItems.Add(timeLineItem);
            
            // Serialize added item
            timeLineItem.SerializationDirectory = _serializationDir;
            timeLineItem.ShouldAutoSerialize = true;
            timeLineItem.SerializeConfigurations(null,null);
        }
        
        private void Delete(TimeLineItemViewModel timeLineItem)
        {
            // Remove from memory
            TimeLineItems.Remove(timeLineItem);
            
            // Remove from disk
            var pathToRemove = Path.Combine(_serializationDir,
                timeLineItem.Name + ".xml");

            if (File.Exists(pathToRemove))
            {
                File.Delete(pathToRemove);
            }
        }

        private void DeleteOutdatedItems()
        {
            var itemsToDelete = (from t in TimeLineItems
                let days = (DateTime.Now - t.StartTime).TotalDays
                where days > DaysExpired
                select t).ToList();
            foreach (var item in itemsToDelete)
            {
                Delete(item);
            }
        }

        #endregion
    }
}