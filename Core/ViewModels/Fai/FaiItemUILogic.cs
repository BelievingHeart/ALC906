namespace Core.ViewModels.Fai
{
    public partial class FaiItem
    {
        public bool TooLarge
        {
            get { return Value > MaxBoundary; }
        }

        public bool TooSmall
        {
            get { return Value < MinBoundary; }
        }
    }
}