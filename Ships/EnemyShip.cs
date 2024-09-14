using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDShooter.Collisions;

namespace TDShooter.Ships
{
    public class EnemyShip
    {
        // the texture of the ship
        private Texture2D _texture;

        // the texture for the projectile
        private Texture2D[] _projectileTexture = new Texture2D[4];

        // the bounding rectangle for the ship
        private BoundingRectangle _hitbox;

        // the angle of the ship
        private float _angle;

        private Random rand;

        // The sound effect for firing the laser
        private SoundEffect _laserSFX;

        /// <summary>
        /// List of all active shots from the given enemy ship
        /// </summary>
        public List<Projectile> Shots = new List<Projectile>();

        // the amount of time between shots
        private double _shotTimer;

        // the delay between shots
        private const double _SHOT_DELAY = 1.5f;

        private const float _MAX_SPEED = 100f;

        /// <summary>
        /// The velocity of the ship
        /// </summary>
        public Vector2 Velocity;

        /// <summary>
        /// The mass of the ship
        /// </summary>
        public float Mass;

        /// <summary>
        /// The position of the spaceship.
        /// </summary>
        public Vector2 Position = new Vector2();

        /// <summary>
        /// The current health of the spaceship
        /// </summary>
        public int Health = 100;

        /// <summary>
        /// The speed modifier of the ship
        /// </summary>
        public float Speed = 2;

        /// <summary>
        /// The attacking damage of the ship
        /// </summary>
        public int Damage = 50;

        /// <summary>
        /// Determines if the ship is deaed (health <= 0)
        /// </summary>
        public bool Dead;

        /// <summary>
        /// Determines if the ship is colliding with another object
        /// </summary>
        public bool IsColliding;

        /// <summary>
        /// The hitbox of the ship
        /// </summary>
        public BoundingRectangle Hitbox => _hitbox;

        /// <summary>
        /// Loads the content for the player ship sprite
        /// </summary>
        /// <param name="content">The content manager to load the sprite with</param>
        public void LoadContent(ContentManager content)
        {
            rand = new Random();
            int enemyType = rand.Next(1, 3);

            _texture = content.Load<Texture2D>("enemy-" + enemyType);
            _projectileTexture[0] = content.Load<Texture2D>("enemy-shot");
            _projectileTexture[1] = content.Load<Texture2D>("enemy-impact-1");
            _projectileTexture[2] = content.Load<Texture2D>("enemy-impact-2");
            _projectileTexture[3] = content.Load<Texture2D>("enemy-impact-3");
            _hitbox = new BoundingRectangle(Position, _texture.Width, _texture.Height);
            _laserSFX = content.Load<SoundEffect>("sfx_laser2");
        }

        /// <summary>
        /// Updates the player ship based on how much gametime has elapsed
        /// </summary>
        /// <param name="gameTime">the current gametime of the game</param>
        public void Update(GameTime gameTime, Vector2 playerPosition)
        {
            if (Dead)
            {
                _hitbox.Width = 0;
                _hitbox.Height = 0;
                _hitbox.X = 0;
                _hitbox.Y = 0;
                return;
            }

            _shotTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (_shotTimer > _SHOT_DELAY) // shoot at the player if its been long enough between shots
            {
                Shoot(playerPosition);
                _laserSFX.Play();
                _shotTimer = 0;
            }

            Vector2 acceleration = new Vector2(0, 0);

            Vector2 direction = new Vector2(playerPosition.X, playerPosition.Y) - Position;

            acceleration += direction * Speed;
            _angle = (float)Math.Atan2(direction.X, -direction.Y);

            Velocity += acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Velocity.Length() > _MAX_SPEED)
            {
                Velocity = Vector2.Normalize(Velocity) * _MAX_SPEED;
            }

            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (IsColliding) // apply a repelling force to the ship if it's colliding with another ship
            {
                
            }

            _hitbox.X = Position.X;
            _hitbox.Y = Position.Y;

            List<Projectile> projectiles = new List<Projectile>(Shots);

            // Update all the active projectiles
            foreach (var p in projectiles)
            {
                if (p.IsActive)
                {
                    p.Update(gameTime);
                }
                else
                {
                    Shots.Remove(p);
                }
            }
        }

        /// <summary>
        /// Draws the sprite
        /// </summary>
        /// <param name="gameTime">The time in the game</param>
        /// <param name="spriteBatch">The spritebatch to draw with</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 origin = new Vector2(_texture.Width / 2, _texture.Height / 2);

            spriteBatch.Draw(_texture, Position, null, Color.White, _angle, origin, 1f, SpriteEffects.FlipVertically, 0f);


            foreach (var p in Shots)
            {
                if (p.IsActive)
                {
                    p.Draw(gameTime, spriteBatch);
                }
            }
        }

        /// <summary>
        /// Handles decreasing the health when the ship has taken damage
        /// </summary>
        /// <param name="damage">the amount of damage to take</param>
        public void TakeDamage(int damage, out bool isDead)
        {
            Health -= damage;

            if (Health <= 0)
            {
                Dead = true;
                isDead = true;
            }
            else
            {
                isDead = false;
            }
            
        }

        /// <summary>
        /// Shoots a projectile from the ship
        /// </summary>
        public void Shoot(Vector2 playerPosition)
        {
            Vector2 direction = new Vector2(playerPosition.X, playerPosition.Y) - Position;
            float angle = (float)Math.Atan2(direction.X, -direction.Y);
            Projectile shot = new Projectile(_projectileTexture, Position, direction, 300, angle);
            Shots.Add(shot);
        }

        public void CheckShotsForCollision(PlayerShip player)
        {
            foreach (var p in Shots)
            {
                if (p.Hitbox.CollidesWith(player.Hitbox) & !p.IsHitting)
                {
                    p.IsHitting = true;
                    player.TakeDamage(Damage);
                }
            }
        }
    }
}
