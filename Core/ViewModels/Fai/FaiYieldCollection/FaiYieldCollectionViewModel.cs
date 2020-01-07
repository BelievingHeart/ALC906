using System;
using System.Collections.Generic;
using System.Linq;
using WPFCommon.ViewModels.Base;

namespace Core.ViewModels.Fai.FaiYieldCollection
{
    public class FaiYieldCollectionViewModel : ViewModelBase
    {
        #region private members

        private int _totalCount = 0;
        public Dictionary<string, int> NgCountDict { get; }

        #endregion

        public event Action YieldsUpdated;



        #region prop

        public Dictionary<string, int> PercentDict { get; set; }
        

        #endregion

        #region ctor

        public FaiYieldCollectionViewModel(IEnumerable<string> faiNames)
        {
            NgCountDict = new Dictionary<string, int>();
            PercentDict = new Dictionary<string, int>();
            foreach (var name in faiNames)
            {
                NgCountDict[name] = 0;
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
                if (faiItem.Rejected) NgCountDict[faiItem.Name]++;
            }
            
            // Calc yields
            foreach (var key in NgCountDict.Keys)
            {
                PercentDict[key] = (int)((_totalCount - NgCountDict[key]) / (double) _totalCount * 100);
            }

            OnYieldsUpdated();
        }
        
        public void Clear()
        {
            _totalCount = 0;
            var faiNames = NgCountDict.Keys.ToArray();
            foreach (var key in faiNames)
            {
                NgCountDict[key] = 0;
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