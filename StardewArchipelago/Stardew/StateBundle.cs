namespace StardewArchipelago.Stardew
{
    public class StateBundle
    {
        public Bundle RelatedBundle { get; }
        public bool IsCompleted { get; }

        public StateBundle(Bundle relatedBundle, bool isCompleted)
        {
            RelatedBundle = relatedBundle;
            IsCompleted = isCompleted;
        }
    }
}
