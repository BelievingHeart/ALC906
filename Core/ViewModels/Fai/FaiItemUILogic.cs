namespace Core.ViewModels.Fai
{
    public partial class FaiItem
    {
        public bool TooLarge => Value > MaxBoundary;

        public bool TooSmall => Value < MinBoundary;
    }
}