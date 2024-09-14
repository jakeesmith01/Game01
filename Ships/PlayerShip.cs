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
    public class PlayerShip
    {
        // the texture of the ship
        private Texture2D _texture;

        // the damage texture to draw on top of the ship when damaged (health <= 50)
        private Texture2D[] _damagedTexture = new Texture2D[3];

        // the texture for the projectile
        private Texture2D[] _projectileTexture = new Texture2D[4];

        // the bounding rectangle for the ship
        private BoundingRectangle _hitbox;

        // the current state of the keyboard
        private KeyboardState _keyboardState;

        // the current state of the mouse
        private MouseState _mouseState;

        // the angle of the ship
        private float _angle;

        // the sound effect for firing the laser
        private SoundEffect _laserSFX;

        // The timer for time between shots
        private float _shotTimer;

        // The delay between shots
        private const float _SHOT_DELAY = 0.7f;

        /// <summary>
        /// List of all active shots from the player
        /// </summary>
        public List<Projectile> Shots = new List<Projectile>();

        /// <summary>
        /// The position of the spaceship.
        /// </summary>
        public Vector2 Position = new Vector2(800, 650);

        /// <summary>
        /// The mass of the ship
        /// </summary>
        public float Mass = 1.0f;

        /// <summary>
        /// The current health of the spaceship
        /// </summary>
        public int Health = 300;

        /// <summary>
        /// The speed modifier of the ship
        /// </summary>
        public float Speed = 5;

        /// <summary>
        /// The attacking damage of the ship
        /// </summary>
        public int Damage = 50;

        /// <summary>
        /// Determines if the ship is deaed (health <= 0)
        /// </summary>
        public bool Dead;

        /// <summary>
        /// The current score of the player
        /// </summary>
        public int Score;

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
            _texture = content.Load<Texture2D>("player");
            _damagedTexture[0] = content.Load<Texture2D>("playerShip1_damage1");
            _damagedTexture[1] = content.Load<Texture2D>("playerShip1_damage2");
            _damagedTexture[2] = content.Load<Texture2D>("playerShip1_damage3");
            _projectileTexture[0] = content.Load<Texture2D>("player-shot-1");
            _projectileTexture[1] = content.Load<Texture2D>("player-impact-1");
            _projectileTexture[2] = content.Load<Texture2D>("player-impact-2");
            _projectileTexture[3] = content.Load<Texture2D>("player-impact-3");
            _hitbox = new BoundingRectangle(Position, _texture.Width, _texture.Height);
            _laserSFX = content.Load<SoundEffect>("sfx_laser2");
        }

        /// <summary>
        /// Updates the player ship based on how much gametime has elapsed
        /// </summary>
        /// <param name="gameTime">the current gametime of the game</param>
        public void Update(GameTime gameTime)
        {
            

            MouseState previousMS = _mouseState;
            _keyboardState = Keyboard.GetState();
            _mouseState = Mouse.GetState();

            if (_keyboardState.IsKeyDown(Keys.W) || _keyboardState.IsKeyDown(Keys.Up)) Position += new Vector2(0, -1) * Speed;
            if (_keyboardState.IsKeyDown(Keys.S) || _keyboardState.IsKeyDown(Keys.Down)) Position += new Vector2(0, 1) * Speed;
            if (_keyboardState.IsKeyDown(Keys.A) || _keyboardState.IsKeyDown(Keys.Left)) Position += new Vector2(-1, 0) * Speed;
            if (_keyboardState.IsKeyDown(Keys.D) || _keyboardState.IsKeyDown(Keys.Right)) Position += new Vector2(1, 0) * Speed;

            _hitbox.X = Position.X;
            _hitbox.Y = Position.Y;

            _shotTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_mouseState.LeftButton == ButtonState.Pressed && previousMS.LeftButton != ButtonState.Pressed && _shotTimer > _SHOT_DELAY)
            {
                Shoot(_mouseState);
                _laserSFX.Play();
                _shotTimer = 0;
            }

            // Calculate the angle between the ship and the mouse cursor
            Vector2 direction = new Vector2(_mouseState.X, _mouseState.Y) - Position;
            _angle = (float)Math.Atan2(direction.X, -direction.Y);

            List<Projectile> projectiles = new List<Projectile>(Shots);

            // Update all the active projectiles
            foreach(var p in projectiles) 
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

            if(Health <= 122.5)
            {
                spriteBatch.Draw(_texture, Position, null, Color.White, _angle, origin, 1f, SpriteEffects.None, 1);
                spriteBatch.Draw(_damagedTexture[2], Position, null, Color.White, _angle, origin, 1f, SpriteEffects.None, 0);
            }
            else if(Health <= 210)
            {
                spriteBatch.Draw(_texture, Position, null, Color.White, _angle, origin, 1f, SpriteEffects.None, 1);
                spriteBatch.Draw(_damagedTexture[1], Position, null, Color.White, _angle, origin, 1f, SpriteEffects.None, 0);
            }
            else if(Health <= 280)
            {
                spriteBatch.Draw(_texture, Position, null, Color.White, _angle, origin, 1f, SpriteEffects.None, 1);
                spriteBatch.Draw(_damagedTexture[0], Position, null, Color.White, _angle, origin, 1f, SpriteEffects.None, 0);
            }
            else
            {
                spriteBatch.Draw(_texture, Position, null, Color.White, _angle, origin, 1f, SpriteEffects.None, 0f);
            }


            foreach(var p in Shots)
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
        public void TakeDamage(int damage)
        {
            Health -= damage;

            if (Health <= 0)
            {
                Dead = true;
            }
        }

        /// <summary>
        /// Shoots a projectile from the ship
        /// </summary>
        public void Shoot(MouseState ms)
        {
            Vector2 direction = new Vector2(ms.X, ms.Y) - Position;
            float angle = (float)Math.Atan2(direction.X, -direction.Y);
            Projectile shot = new Projectile(_projectileTexture, Position, direction, 500, angle);
            Shots.Add(shot);
        }

        public void CheckShotsForCollision(EnemyShip[] enemies)
        {
            bool enemyDead = false;

            foreach (var p in Shots)
            {
                foreach (var enemy in enemies)
                {
                    if (p.Hitbox.CollidesWith(enemy.Hitbox) && !p.IsHitting)
                    {
                        enemyDead = false;
                        p.IsHitting = true;
                        enemy.TakeDamage(Damage, out enemyDead);

                        if (enemyDead)
                        {
                            Score += 1;
                        }
                    }
                }
            }
        }
    }
}
