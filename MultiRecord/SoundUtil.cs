using NAudio.Wave;
using System;
using System.IO;

namespace MultiRecord
{
    public class SoundUtil
    {
        private static IWavePlayer waveOut;
        private static AudioFileReader audioFile;

        public static void Beep()
        {
            string soundPath = SettingsManager.GetSoundPath("Beep");
            PlayCustomSound(soundPath);
        }

        public static void Delete()
        {
            string soundPath = SettingsManager.GetSoundPath("Delete");
            PlayCustomSound(soundPath);
        }

        public static void Over()
        {
            string soundPath = SettingsManager.GetSoundPath("Over");
            PlayCustomSound(soundPath);
        }

        public static void PlayCustomSound(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"ไม่พบไฟล์เสียง: {filePath}");
                    return;
                }

                // กำจัด instance เก่าถ้ามี
                waveOut?.Stop();
                waveOut?.Dispose();
                audioFile?.Dispose();

                // โหลดและเล่นไฟล์เสียง
                audioFile = new AudioFileReader(filePath);
                waveOut = new WaveOutEvent();
                waveOut.Init(audioFile);
                waveOut.Play();
            }
            catch (Exception ex)
            {
                // ถ้าไม่ต้องการ Error เด้ง ให้ log หรือเงียบไว้
                Console.WriteLine("เล่นเสียงไม่สำเร็จ: " + ex.Message);
            }
        }

        public static void Dispose()
        {
            try
            {
                waveOut?.Stop();
                waveOut?.Dispose();
                audioFile?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ปิดการใช้งานเสียงไม่สำเร็จ: " + ex.Message);
            }
        }
    }
}
