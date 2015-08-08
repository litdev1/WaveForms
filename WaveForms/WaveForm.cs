using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SmallBasic.Library;
using SlimDX;
using SlimDX.DirectSound;
using SlimDX.Multimedia;
using System.Runtime.InteropServices;
using SBArray = Microsoft.SmallBasic.Library.Array;

//Based on https://www.insecure.ws/2010/03/09/control-rc-aircrafts-from-your-computer-for-0

namespace WaveForms
{
    /// <summary>
    /// Create simple sound waveforms
    /// </summary>
    [SmallBasicType]
    public static class WaveForm
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private static DirectSound directSound = null;
        private static WaveFormat waveFormat;
        private static short amplitude = 20262;

        /// <summary>
        /// Signal amplitude (maximum is 2^15 = 32768, default is 20262)
        /// </summary>
        public static Primitive Amplitude
        {
            get { return (int)amplitude; }
            set { amplitude = (short)value; }
        }

        /// <summary>
        /// Play a Sine wave form.
        /// </summary>
        /// <param name="frequency">Frequency (HZ)</param>
        /// <param name="duration">Duration (ms)</param>
        public static void PlaySineWave(Primitive frequency, Primitive duration)
        {
            Play(frequency, duration / 1000.0, 1);
        }

        /// <summary>
        /// Play a square wave form.
        /// </summary>
        /// <param name="frequency">Frequency (HZ)</param>
        /// <param name="duration">Duration (ms)</param>
        public static void PlaySquareWave(Primitive frequency, Primitive duration)
        {
            Play(frequency, duration / 1000.0, 2);
        }

        /// <summary>
        /// Play DX7.
        /// </summary>
        /// <param name="channels">An array of values for each channel (values between 0 and 1, usually 8 channels).</param>
        public static void PlayDX7(Primitive channels)
        {
            Initialise();
            try
            {
                int i, iServo;
                double duration = 0.0225;
                int sampleCount = (int)(duration * waveFormat.SamplesPerSecond);

                // buffer description         
                SoundBufferDescription soundBufferDescription = new SoundBufferDescription();
                soundBufferDescription.Format = waveFormat;
                soundBufferDescription.Flags = BufferFlags.Defer;
                soundBufferDescription.SizeInBytes = sampleCount * waveFormat.BlockAlignment;

                SecondarySoundBuffer secondarySoundBuffer = new SecondarySoundBuffer(directSound, soundBufferDescription);

                short[] rawsamples = new short[sampleCount];
                int stopSamples = (int)(0.0004 * waveFormat.SamplesPerSecond);
                List<int> servoSamples = new List<int>();
                Primitive indices = SBArray.GetAllIndices(channels);
                int servoCount = SBArray.GetItemCount(indices);
                for (iServo = 1; iServo <= servoCount; iServo++)
                {
                    servoSamples.Add((int)((0.0007 + 0.0008 * channels[indices[iServo]]) * waveFormat.SamplesPerSecond));
                }
                //Lead-in
                int leading = sampleCount - (servoCount + 1) * stopSamples - servoSamples.Sum();
                int sample = 0;
                for (i = 0; i < leading; i++) rawsamples[sample++] = 0;
                //Servos
                for (i = 0; i < stopSamples; i++) rawsamples[sample++] = (short)(-amplitude);
                for (iServo = 0; iServo < servoCount; iServo++)
                {
                    for (i = 0; i < servoSamples[iServo]; i++) rawsamples[sample++] = amplitude;
                    for (i = 0; i < stopSamples; i++) rawsamples[sample++] = (short)(-amplitude);
                }

                //load audio samples to secondary buffer
                secondarySoundBuffer.Write(rawsamples, 0, LockFlags.EntireBuffer);

                //play audio buffer			
                secondarySoundBuffer.Play(0, PlayFlags.None);

                //wait to complete before returning
                while ((secondarySoundBuffer.Status & BufferStatus.Playing) != 0);

                secondarySoundBuffer.Dispose();
            }
            catch (Exception ex)
            {
                TextWindow.WriteLine(ex.Message);
            }
        }

        private static void Play(double frequency, double duration, int iType)
        {
            Initialise();
            try
            {
                int sampleCount = (int)(duration * waveFormat.SamplesPerSecond);

                // buffer description         
                SoundBufferDescription soundBufferDescription = new SoundBufferDescription();
                soundBufferDescription.Format = waveFormat;
                soundBufferDescription.Flags = BufferFlags.Defer;
                soundBufferDescription.SizeInBytes = sampleCount * waveFormat.BlockAlignment;

                SecondarySoundBuffer secondarySoundBuffer = new SecondarySoundBuffer(directSound, soundBufferDescription);

                short[] rawsamples = new short[sampleCount];
                double frac, value;

                switch (iType)
                {
                    case 1: //Sinusoidal
                        for (int i = 0; i < sampleCount; i++)
                        {
                            frac = frequency * duration * i / (double)sampleCount;
                            value = System.Math.Sin(2.0 * System.Math.PI * frac);
                            rawsamples[i] = (short)(amplitude * value);
                        }
                        break;
                    case 2: //Square
                        for (int i = 0; i < sampleCount; i++)
                        {
                            frac = frequency * duration * i / (double)sampleCount;
                            frac = frac - (int)frac;
                            value = frac < 0.5 ? -1.0 : 1.0;
                            rawsamples[i] = (short)(amplitude * value);
                        }
                        break;
                }

                //load audio samples to secondary buffer
                secondarySoundBuffer.Write(rawsamples, 0, LockFlags.EntireBuffer);

                //play audio buffer			
                secondarySoundBuffer.Play(0, PlayFlags.None);

                //wait to complete before returning
                while ((secondarySoundBuffer.Status & BufferStatus.Playing) != 0);

                secondarySoundBuffer.Dispose();
            }
            catch (Exception ex)
            {
                TextWindow.WriteLine(ex.Message);
            }
        }

        private static void Initialise()
        {
            try
            {
                if (directSound == null)
                {
                    //Initialize the DirectSound Device
                    directSound = new DirectSound();

                    waveFormat = new WaveFormat();
                    waveFormat.BitsPerSample = 16;
                    waveFormat.Channels = 1;
                    waveFormat.BlockAlignment = (short)(waveFormat.BitsPerSample / 8);

                    waveFormat.FormatTag = WaveFormatTag.Pcm;
                    waveFormat.SamplesPerSecond = 192000;
                    waveFormat.AverageBytesPerSecond = waveFormat.SamplesPerSecond * waveFormat.BlockAlignment;
                }
                // Set the priority of the device with the rest of the operating system
                directSound.SetCooperativeLevel(GetForegroundWindow(), CooperativeLevel.Priority);
            }
            catch (Exception ex)
            {
                TextWindow.WriteLine(ex.Message);
            }
        }
    }
}
