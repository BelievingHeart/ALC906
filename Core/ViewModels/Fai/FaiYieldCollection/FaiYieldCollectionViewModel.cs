using System;
using System.Collections.Generic;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Fai.FaiYieldCollection
{
    public class FaiYieldCollectionViewModel : ViewModelBase
    {
        #region private members

        private int _totalCount = 0;
        private Dictionary<string, int> _okCountDict;

        #endregion

        public event Action YieldsUpdated;



        #region prop

        public Dictionary<string, int> PercentDict { get; set; }
        

        #endregion

        #region ctor

        public FaiYieldCollectionViewModel(IEnumerable<string> faiNames)
        {
            _okCountDict = new Dictionary<string, int>();
            PercentDict = new Dictionary<string, int>();
            foreach (var name in faiNames)
            {
                _okCountDict[name] = 0;
                PercentDict[name] = 100;
            }
        }

        #endregion

        #region api

        public void Update(IEnumerable<FaiItem> faiItems)
        {
            _totalCount++;
            
            // Increment ok counts
            foreach (var faiItem in faiItems)
            {
                if (faiItem.Passed) _okCountDict[faiItem.Name]++;
            }
            
            // Calc yields
            foreach (var key in _okCountDict.Keys)
            {
                PercentDict[key] = (int)(_okCountDict[key] / (double) _totalCount * 100);
            }

            OnYieldsUpdated();
        }
        
        public void Clear()
        {
            _totalCount = 0;
            foreach (var key in _okCountDict.Keys)
            {
                _okCountDict[key] = 0;
                PercentDict[key] = 100;
            }
            OnYieldsUpdated();
        }

        #endregion

        protected virtual void OnYieldsUpdated()
        {
            YieldsUpdated?.Invoke();
        }
    }
}