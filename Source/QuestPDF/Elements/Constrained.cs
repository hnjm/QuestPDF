using System;
using QuestPDF.Drawing;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QuestPDF.Elements
{
    internal sealed class Constrained : ContainerElement, IContentDirectionAware
    {
        public ContentDirection ContentDirection { get; set; }
        
        public float? MinWidth { get; set; }
        public float? MaxWidth { get; set; }

        public float? MinHeight { get; set; }
        public float? MaxHeight { get; set; }

        public bool EnforceSizeWhenEmpty { get; set; }
        
        internal override SpacePlan Measure(Size availableSpace)
        {
            if (!EnforceSizeWhenEmpty && Child.IsEmpty())
                return SpacePlan.Empty();
            
            if (MinWidth > availableSpace.Width + Size.Epsilon)
                return SpacePlan.Wrap();
            
            if (MinHeight > availableSpace.Height + Size.Epsilon)
                return SpacePlan.Wrap();
            
            var available = new Size(
                Min(MaxWidth, availableSpace.Width),
                Min(MaxHeight, availableSpace.Height));

            var measurement = base.Measure(available);

            if (measurement.Type == SpacePlanType.Wrap)
                return SpacePlan.Wrap();
            
            var actualSize = new Size(
                Max(MinWidth, measurement.Width),
                Max(MinHeight, measurement.Height));
            
            if (measurement.Type == SpacePlanType.Empty)
                return EnforceSizeWhenEmpty ? SpacePlan.FullRender(actualSize) : SpacePlan.Empty();
            
            if (measurement.Type == SpacePlanType.FullRender)
                return SpacePlan.FullRender(actualSize);
            
            if (measurement.Type == SpacePlanType.PartialRender)
                return SpacePlan.PartialRender(actualSize);
            
            throw new NotSupportedException();
        }
        
        internal override void Draw(Size availableSpace)
        {
            var size = new Size(
                Min(MaxWidth, availableSpace.Width),
                Min(MaxHeight, availableSpace.Height));
            
            var offset = ContentDirection == ContentDirection.LeftToRight
                ? Position.Zero
                : new Position(availableSpace.Width - size.Width, 0);
            
            Canvas.Translate(offset);
            base.Draw(size);
            Canvas.Translate(offset.Reverse());
        }
        
        private static float Min(float? x, float y)
        {
            return x.HasValue ? Math.Min(x.Value, y) : y; 
        }
        
        private static float Max(float? x, float y)
        {
            return x.HasValue ? Math.Max(x.Value, y) : y;
        }
    }
}