using System.Collections.Generic;
using Demon;
using HalconDotNet;

namespace Core.ImageProcessing._2D
{
    public class I40_CheckDemo
    {
        
        
        // 运行一次图像处理
        public void OnGetCheckValue(List<HImage> srcImage1, int posIndex, int showSelect, HWindow hWindow)
        {
            
        }
        public int resultNum;
        public ResultValueClass[] myResult = null;

        
        // 读取/保存搜边参数
        public List<string> SearchLineHeader;
        public Dictionary<string, List<string>> SearchLineDictionary;
        public void SaveSearchLineParam()
        {
            
        }

        // 读取/保存结果
        public List<string> ResultHeader;
        public Dictionary<string, List<string>> ResultDictionary1;
        public void SaveResultLimitParam()
        {
            
        }

        // 读取/保存其他算法参数
        public List<string> AlgHeader;
        public Dictionary<string, List<string>> AlgDictionary;

        public void SaveAlgParam()
        {
            
        }
    }
}