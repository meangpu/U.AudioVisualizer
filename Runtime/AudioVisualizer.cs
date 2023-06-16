using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Linq;

namespace Meangpu.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioVisualizer : MonoBehaviour
    {
        [SerializeField] Color _imgColor;

        [SerializeField] float _minHeight = 14;
        [SerializeField] float _maxHeight = 44;
        [SerializeField] float _sizeChangeSensitivity = 5;
        [SerializeField] float _smoothDeltaTime = 0.5f;

        [SerializeField] AudioSource _audioSource;
        [Tooltip("use audio mixer group to mute microphone")]
        [SerializeField] AudioMixerGroup _mixer;
        [SerializeField] AudioClip _audioClip;
        [SerializeField] bool _isUsingMic;
        [SerializeField] string _micName;

        [Space(20)]
        [SerializeField] Image[] _visualizerObjImage;
        [SerializeField] RectTransform[] _visualizerObjRectTrans;
        [SerializeField] float[] _spectrumData = new float[64];

        private void Start()
        {
            _visualizerObjRectTrans = transform.GetComponentsInChildren<RectTransform>().Skip(1).ToArray();
            _visualizerObjImage = transform.GetComponentsInChildren<Image>().Skip(1).ToArray();

            SetVisualizerColor();
            SetAudioSource();
        }

        void SetAudioSource()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.outputAudioMixerGroup = _mixer;

            if (_isUsingMic)
            {
                SetMicrophoneSource();
            }
            else
            {
                _audioSource.clip = _audioClip;
            }
            _audioSource.Play();
        }

        private void SetMicrophoneSource()
        {
            if (Microphone.devices.Length > 0)
            {
                _micName = Microphone.devices[0];
                _audioSource.clip = Microphone.Start(_micName, true, 10, AudioSettings.outputSampleRate);
                _audioSource.loop = true;
                while (Microphone.GetPosition(null) <= 0) { }
            }
        }

        public void SetVisualizerColor()
        {
            foreach (var Image in _visualizerObjImage)
            {
                Image.color = _imgColor;
            }
        }

        private void Update()
        {
            _audioSource.GetSpectrumData(_spectrumData, 0, FFTWindow.Rectangular);
            for (var i = 0; i < _visualizerObjRectTrans.Length; i++)
            {
                Vector2 newSize = _visualizerObjRectTrans[i].rect.size;
                float targetSize = Mathf.Clamp(_minHeight + (_spectrumData[i] * (_maxHeight - _minHeight) * _sizeChangeSensitivity), _minHeight, _maxHeight);

                newSize.y = Mathf.Lerp(newSize.y, targetSize, _smoothDeltaTime);
                _visualizerObjRectTrans[i].sizeDelta = newSize;
            }
        }
    }
}