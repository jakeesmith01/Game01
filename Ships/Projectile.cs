using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDShooter.Collisions;

namespace TDShooter.Ships
{
    public class Projectile
    {
        /// <summary>
        /// Position of the projectile
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// Velocity of the projectile
        /// </summary>
        public Vector2 Velocity;

        // The lifespan of the projectile
        private const float _LIFESPAN = 6f;

        private const float _ANIMATION_SPEED = 0.1f;

        // The amount of time the projectile has been alive
        private float _timeAlive = 0f;

        // The amount of time between animation frames
        private float _animationTimer;

        // The current frame of the animation
        private short _animationFrame;

        /// <summary>
        /// the bounding rectangle for the projectile
        /// </summary>
        private BoundingRectangle _hitbox;

        /// <summary>
        /// The bounding rectangle for the projectile
        /// </summary>
        public BoundingRectangle Hitbox => _hitbox;

        /// <summary>
        /// Determines if the projectile is hitting something
        /// </summary>
        public bool IsHitting { get; set; }

        /// <summary>
        /// Determines if the projectile is active or not
        /// </summary>
        public bool IsActive = true;

        /// <summary>
        /// the texture for the projectile
        /// </summary>
        private Texture2D[] _texture;

        /// <summary>
        /// the speed of the projectile
        /// </summary>
        private float _speed;

        /// <summary>
        /// the angle the projectile was fired at 
        /// </summary>
        private float _angle;

        /// <summary>
        /// A projectile to be shot from a ship
        /// </summary>
        /// <param name="texture">The texture of the projectile</param>
        /// <param name="position">The position of the projectile</param>
        /// <param name="direction">The direction of the projectile</param>
        /// <param name="speed">The speed of the projectile</param>
        public Projectile(Texture2D[] texture, Vector2 position, Vector2 direction, float speed, float angle)
        {
            _texture = texture;
            Position = position;
            _speed = speed;
            _angle = angle;
            Velocity = Vector2.Normalize(direction) * _speed;
            _hitbox = new BoundingRectangle(Position, _texture[0].Width, _texture[0].Height);
        }

        /// <summary>
        /// Updates the projectile based off the gametime
        /// </summary>
        /// <param name="gameTime">the current time in the game</param>
        public void Update(GameTime gameTime)
        {
            if (!IsHitting)
            {
                Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

                _hitbox.X = Position.X;
                _hitbox.Y = Position.Y;

                _timeAlive += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_timeAlive >= _LIFESPAN)
                {
                    IsActive = false;
                }
            }
        }

        /// <summary>
        /// Draws the projectile
        /// </summary>
        /// <param name="spriteBatch">the spritebatch to draw with</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 origin = new Vector2(_texture[0].Width / 2, _texture[0].Height / 2);

            if(IsHitting) // if the projectile is hitting something, animate the impact
            {
                _animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if(_animationTimer >= _ANIMATION_SPEED)
                {
                    _animationFrame++;
                    _animationTimer -= _ANIMATION_SPEED;
                }

                if(_animationFrame <= _texture.Length - 1)
                {
                    Vector2 impactOrigin = new Vector2(_texture[_animationFrame].Width / 2, _texture[_animationFrame].Height / 2);
                    spriteBatch.Draw(_texture[_animationFrame], Position, null, Color.White, _angle, origin, 2.3f, SpriteEffects.None, 0.1f);
                }
                else
                {
                    IsActive = false;
                }

               
            }
            else // otherwise continue drawing the standard shot texture
            {
                spriteBatch.Draw(_texture[0], Position, null, Color.White, _angle, origin, 1f, SpriteEffects.None, 0f);
            }



        }

      
    }
}
