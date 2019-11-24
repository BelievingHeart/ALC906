using System.Collections.Generic;
using Core.ViewModels.Application;
using Core.ViewModels.Fai;
using WPFCommon.ViewModels.Base;

namespace Core.DesignTime
{
    public class FaiItemsListViewDesignModel : ViewModelBase
    {
        private List<FaiItem> _faiItems;
        public static FaiItemsListViewDesignModel Instance { get; set; }

        public List<FaiItem> FaiItems
        {
            get
            {
                return _faiItems;
                
            }
            set { _faiItems = value; }
        }

        public FaiItemsListViewDesignModel()
        {
            FaiItems = new List<FaiItem>()
            {
                new FaiItem()
                {
                    Name = "2.1", Bias = 1, MaxBoundary = 100, MinBoundary = 0, ValueUnbiased = 100, Weight = 1
                },
                new FaiItem()
                {
                    Name = "2.2", Bias = 1, MaxBoundary = 100, MinBoundary = 0, ValueUnbiased = -100, Weight = 1
                },
                new FaiItem()
                {
                    Name = "2.3", Bias = 1, MaxBoundary = 100, MinBoundary = 0, ValueUnbiased = 50, Weight = 1
                },
            };
        }
    }
}