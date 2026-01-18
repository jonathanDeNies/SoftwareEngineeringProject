using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareEngineeringProject
{
    class Animation
    {
        public AnimationFrame CurrentFrame { get; set; }
        private List<AnimationFrame> frames;
        private int counter;

        public Animation()
        {
            frames = new List<AnimationFrame>();
        }

        public void AddFrame(AnimationFrame frame)
        {
            frames.Add(frame);
            CurrentFrame = frames[0];
        }

        private double secondCounter = 0;

        // Only advance the animation when isMoving == true.
        // When not moving we reset to the first (idle) frame.
        public void Update(GameTime gameTime, bool isMoving)
        {
            if (frames.Count == 0) return;

            if (!isMoving)
            {
                // show idle frame when not moving
                counter = 0;
                secondCounter = 0;
                CurrentFrame = frames[0];
                return;
            }

            // moving -> advance animation as before
            secondCounter += gameTime.ElapsedGameTime.TotalSeconds;
            int fps = 8;
            double interval = 1d / fps;
            if (secondCounter >= interval)
            {
                secondCounter = 0;
                counter++;
                if (counter >= frames.Count)
                    counter = 0;
            }

            CurrentFrame = frames[counter];
        }
    }
}