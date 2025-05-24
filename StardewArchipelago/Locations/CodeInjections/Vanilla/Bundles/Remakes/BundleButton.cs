using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Remakes
{
    public class BundleButton : ClickableTextureComponent
    {
        private Vector2 _drawOffset = Vector2.Zero;

        private Texture2D _currentAnimationTexture;
        private Rectangle _currentAnimationBounds;
        private Vector2 _currentAnimationOffsetPerFrame;
        private int _currentAnimationFramesRemaining;
        private Action _actionAfterAnimation;

        private Texture2D _pressAnimationTexture;
        private Rectangle _pressAnimationBounds;
        private Vector2 _pressAnimationOffsetPerFrame;
        private int _pressAnimationFrames;
        private int _animationDelay = 16;

        public BundleButton(string name, Rectangle bounds, string label, string hoverText, Texture2D texture, Rectangle sourceRect, float scale, bool drawShadow = false) : base(name, bounds, label, hoverText, texture, sourceRect, scale, drawShadow)
        {
            // This constructor is not used
        }

        public BundleButton(Rectangle bounds, Texture2D texture, Rectangle sourceRect, float scale, bool drawShadow = false) : base(bounds, texture, sourceRect, scale, drawShadow)
        {
            _actionAfterAnimation = null;
        }

        public void SetDrawOffset(Vector2 offset)
        {
            _drawOffset = offset;
        }

        public void SetPressAnimation(Texture2D animationTexture, Rectangle startingAnimationBounds, Vector2 animationOffsetPerFrame, int animationFrames)
        {
            _pressAnimationTexture = animationTexture;
            _pressAnimationBounds = startingAnimationBounds;
            _pressAnimationOffsetPerFrame = animationOffsetPerFrame;
            _pressAnimationFrames = animationFrames;
        }

        public void StartAnimation(Action actionAfterAnimation = null)
        {
            StartAnimation(_pressAnimationTexture, _pressAnimationBounds, _pressAnimationOffsetPerFrame, _pressAnimationFrames, actionAfterAnimation);
        }

        public void StartAnimation(Texture2D animationTexture, Rectangle startingAnimationBounds, Vector2 animationOffsetPerFrame, int animationFrames, Action actionAfterAnimation = null)
        {
            _currentAnimationTexture = animationTexture;
            _currentAnimationBounds = startingAnimationBounds;
            _currentAnimationOffsetPerFrame = animationOffsetPerFrame;
            _currentAnimationFramesRemaining = animationFrames * _animationDelay;
            _actionAfterAnimation = actionAfterAnimation;
        }

        public bool IsAnimating()
        {
            return _currentAnimationFramesRemaining > 0;
        }

        public override void draw(SpriteBatch b, Color c, float layerDepth, int frameOffset = 0, int xOffset = 0, int yOffset = 0)
        {
            var position = new Vector2(bounds.X + xOffset + sourceRect.Width / 2 * baseScale, bounds.Y + yOffset + sourceRect.Height / 2 * baseScale);
            if (_currentAnimationFramesRemaining <= 0)
            {
                if (_actionAfterAnimation != null)
                {
                    _actionAfterAnimation();
                    _actionAfterAnimation = null;
                }
                // base.draw(b, c, layerDepth, frameOffset, xOffset, yOffset);
                b.Draw(this.texture, position + _drawOffset, sourceRect, c, 0.0f, new Vector2(this.sourceRect.Width / 2, this.sourceRect.Height / 2), this.scale, SpriteEffects.None, layerDepth);
                return;
            }

            b.Draw(_currentAnimationTexture, position + _drawOffset, _currentAnimationBounds, c, 0.0f, new Vector2(sourceRect.Width / 2, sourceRect.Height / 2), scale, SpriteEffects.None, layerDepth);
            NextAnimationFrame();
        }

        private void NextAnimationFrame()
        {
            _currentAnimationFramesRemaining--;
            if (_currentAnimationFramesRemaining % _animationDelay == 0)
            {
                _currentAnimationBounds = new Rectangle(_currentAnimationBounds.X + (int)_currentAnimationOffsetPerFrame.X,
                    _currentAnimationBounds.Y + (int)_currentAnimationOffsetPerFrame.Y, 
                    _currentAnimationBounds.Width, _currentAnimationBounds.Height);
            }
        }
    }
}
