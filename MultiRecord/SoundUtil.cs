using NAudio.Wave;
using System;

public class SoundUtil
{
    private static IWavePlayer waveOut;
    private static AudioFileReader audioFile;

    public static void Beep()
    {
        try
        {
            // กำจัด instance เก่าถ้ามี
            waveOut?.Stop();
            waveOut?.Dispose();
            audioFile?.Dispose();

            // โหลดและเล่นไฟล์เสียง
            audioFile = new AudioFileReader("sound/beep.mp3");
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

    public static void Delete()
    {
        try
        {
            // กำจัด instance เก่าถ้ามี
            waveOut?.Stop();
            waveOut?.Dispose();
            audioFile?.Dispose();

            // โหลดและเล่นไฟล์เสียง
            audioFile = new AudioFileReader("sound/delete.mp3");
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

    public static void Over()
    {
        try
        {
            // กำจัด instance เก่าถ้ามี
            waveOut?.Stop();
            waveOut?.Dispose();
            audioFile?.Dispose();

            // โหลดและเล่นไฟล์เสียง
            audioFile = new AudioFileReader("sound/over.mp3");
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
}
