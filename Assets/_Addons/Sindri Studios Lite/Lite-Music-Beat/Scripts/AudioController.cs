//09/JUL/2020
//José pablo Peñaloza Cobos || Mariana Gutiérrez Carreto
//Obtains the frequencies values of an audio file. 

//References: https://www.youtube.com/watch?v=5pmoP1ZOoNs&list=PL3POsQzaCw53p2tA6AWf7_AWgplskR0Vo&index=1

//Get the complete version of Music Beat at: https://assetstore.unity.com/packages/tools/audio/music-beat-audio-visualizer-192722

//Theory
/*
 * The human ear can hear up to 22,050 frequencies.
 * From those 22,050, we can obtain 512 bands. 
 * This means that each band has 43 frequencies that won´t be lost. (1 Hertz = one cicle per second)
 *
 * If we want the different bandwidth we get the following information: 
 * 
 *   |    Range    |  |number of bands that each group stores |   
 * 1) 20 - 60       Hz -> 2
 * 2) 60 - 250      Hz -> 4
 * 3) 250 - 500     Hz -> 8
 * 4) 500 - 2000    Hz -> 16
 * 5) 2000 - 4000   Hz -> 32
 * 6) 4000 - 6000   Hz -> 64
 * 7) 6000 - 20,000 Hz -> 128
 *                     -> 256 (The last frequency group stores two bands)
 *
 *Get the complete version of MUSIC BEAT for 16 bands.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

[RequireComponent(typeof(AudioSource))]//Generates an Audio Source component in the game object
public class AudioController : MonoBehaviour
{

    //Microphone management
    public AudioClip audioClip;                   //Audio Source.
    public bool useMicrophone;                    //Boolean that indicates if microphone will be used.
    public string selectedDevice;                 //Microphone's name.
    public AudioMixerGroup audioGroupMaster;      //Master audio mixer group
    public AudioMixerGroup audioGroupMicrophone;  //Microphone audio mixer group


    AudioSource audioSource;                      //Object that stores the audio

    float[] samples    = new float[512];   //Array that will store the audio spectrum registers.

    float[] freqBand   = new float[8]; //Array that stores the audio bands without smoothing

    public float[] bandBuffer = new float[8];//Array that stores the smoothed audio bands

    public float[] audioBandBuffer = new float[8]; //Array that stores the normalized values
    
    float[] freqBandMax = new float[8]; //Array that stores the current maximum value obtained. 

    [HideInInspector]
    public float[] bufferDecrease = new float[8];

    
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (useMicrophone)
        {
            if (Microphone.devices.Length > 0)
            {
                selectedDevice = Microphone.devices[0];
                audioSource.outputAudioMixerGroup = audioGroupMicrophone;                                        //Sets the group for the audio source
                audioSource.clip = Microphone.Start(selectedDevice, true, 1, AudioSettings.outputSampleRate);
            }
            else
                useMicrophone = false;
        }
        if (!useMicrophone)
        {
            //Gets the Audio Component
            audioSource.outputAudioMixerGroup = audioGroupMaster;
            audioSource.clip = audioClip;
        }

        audioSource.Play();

        for (int i = 0; i < 8; i++) //Fills of zeros the max array
            freqBandMax[i] = 0;
    }

    // Update is called once per frame
    void Update()
    {
        GetSpectrumAudioSource();
        MakeFrequencyBand();
        BandBuffer();
        NomralizeBufferBand();
    }

    void NomralizeBufferBand()
    {//Function that normalizes de band buffer values
        for (int i = 0; i < 8; i++)
        {
            if (bandBuffer[i] > freqBandMax[i])
            {
                freqBandMax[i] = freqBand[i];
            }
            freqBand[i] = freqBand[i] / freqBandMax[i];
            audioBandBuffer[i] = bandBuffer[i] / freqBandMax[i];
        }
    }

    void GetSpectrumAudioSource()
    {//Function that gets the frequencies and stores them in samples every time it gets called. 
        audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);
    }

    void BandBuffer()
    {
        for(int i = 0; i < 8; ++i)
        {
            if (freqBand[i] > bandBuffer[i])
            {
                bandBuffer[i] = freqBand[i];
                bufferDecrease[i] = 0.005f;
            }
            if (freqBand[i]< bandBuffer[i])
            {
                bandBuffer[i] -= bufferDecrease[i];
                //1.2
                bufferDecrease[i] *= 1.2f;
            }
        }
    }

    void MakeFrequencyBand()
    {
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            float average = 0; //Stores the average of the band group

            int sampleCount = (int)Mathf.Pow(2, i); //Counts the number of bans that each group will receive. It is used for the 'for' cycle. 

            for (int j = 0; j < sampleCount; j++)
            {
                average += samples[count] * (count + 1);//Gets the average
                count++;
            }

            average = average / count;

            freqBand[i] = average * 10;
        }
    }
}
