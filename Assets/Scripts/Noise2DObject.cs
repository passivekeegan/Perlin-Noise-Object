using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise2DObject : ScriptableObject
{
	[SerializeField] private int _layers;
	[SerializeField] private float _total_influence;
	[SerializeField] private List<int> _octaves;
	[SerializeField] private List<int> _influence;
	[SerializeField] private List<float> _persistence;
	[SerializeField] private List<Vector2> _lacunarity;
	[SerializeField] private List<Vector2> _offset;
	[SerializeField] private List<Vector2> _scale;
	
	public void ResetObject()
	{
		_layers = 1;
		_total_influence = 1;
		_octaves = new List<int>() { _default_octaves };
		_influence = new List<int>() { _default_influence };
		_persistence = new List<float>() { _default_persistence };
		_lacunarity = new List<Vector2>() { _default_lacunarity };
		_offset = new List<Vector2>() { _default_offset };
		_scale = new List<Vector2>() { _default_scale };
	}

	#region Modify Layers
	public void ResetLayer(int layer)
	{
		if (layer < 0 || layer >= _layers)
		{
			throw new System.ArgumentOutOfRangeException("layer");
		}
		_octaves[layer] = _default_octaves;
		_influence[layer] = _default_influence;
		_persistence[layer] = _default_persistence;
		_lacunarity[layer] = _default_lacunarity;
		_offset[layer] = _default_offset;
		_scale[layer] = _default_scale;
		//recalculate total influence
		_total_influence = CalculateTotalInfluence();
	}

	public void Addlayer()
	{
		_octaves.Add(_default_octaves);
		_influence.Add(_default_influence);
		_persistence.Add(_default_persistence);
		_lacunarity.Add(_default_lacunarity);
		_offset.Add(_default_offset);
		_scale.Add(_default_scale);
		_layers += 1;
		//recalculate total influence
		_total_influence = CalculateTotalInfluence();
	}

	public void Deletelayer(int layer)
	{
		if (layer < 0 || layer >= _layers)
		{
			throw new System.ArgumentOutOfRangeException("layer");
		}
		if (_layers < 2)
		{
			ResetLayer(0);
		}
		else
		{
			_octaves.RemoveAt(layer);
			_influence.RemoveAt(layer);
			_persistence.RemoveAt(layer);
			_lacunarity.RemoveAt(layer);
			_offset.RemoveAt(layer);
			_scale.RemoveAt(layer);
			_layers -= 1;
			//recalculate total influence
			_total_influence = CalculateTotalInfluence();
		}
	}
	#endregion

	#region Sampling
	public float SampleNoise(Vector2 point)
	{
		if (_total_influence <= 0)
		{
			return 0f;
		}
		float sample = 0f;
		for (int k = 0;k < _layers;k++)
		{
			float factor = _influence[k] / _total_influence;
			sample += factor * SampleLayer(k, point);
		}
		return sample;
	}

	public float SampleLayer(int layer, Vector2 point)
	{
		if (layer < 0 || layer >= _layers)
		{
			throw new System.ArgumentOutOfRangeException("layer");
		}
		Vector2 scale = _scale[layer];
		if (scale.x == 0 || scale.y == 0)
		{
			return 0f;
		}
		int octaves = _octaves[layer];
		float persistence = _persistence[layer];
		Vector2 lacunarity = _lacunarity[layer];
		Vector2 offset = _offset[layer];
		
		float sample = 0f;
		float amplitude = 1;
		Vector2 frequency = Vector2.one;
		for (int i = 0; i < octaves; i++)
		{
			Vector2 sample_point = ((frequency * point) / scale) + offset;
			sample += amplitude * Mathf.Clamp01(Mathf.PerlinNoise(sample_point.x, sample_point.y));
			amplitude *= persistence;
			frequency *= lacunarity;
		}
		return sample;
	}
	#endregion

	#region GenerateNoiseImage
	public Texture2D GenerateNoiseImage(int width, int height)
	{
		return GenerateNoiseImage(width, height, TextureFormat.RGB24, true, false);
	}
	public Texture2D GenerateNoiseImage(int layer, int width, int height)
	{
		return GenerateNoiseImage(layer, width, height, TextureFormat.RGB24, true, false);
	}


	public Texture2D GenerateNoiseImage(int width, int height, bool inverse)
	{
		return GenerateNoiseImage(width, height, TextureFormat.RGB24, true, inverse);
	}
	public Texture2D GenerateNoiseImage(int layer, int width, int height, bool inverse)
	{
		return GenerateNoiseImage(layer, width, height, TextureFormat.RGB24, true, inverse);
	}


	public Texture2D GenerateNoiseImage(int width, int height, TextureFormat format)
	{
		return GenerateNoiseImage(width, height, format, true, false);
	}
	public Texture2D GenerateNoiseImage(int layer, int width, int height, TextureFormat format)
	{
		return GenerateNoiseImage(layer, width, height, format, true, false);
	}

	public Texture2D GenerateNoiseImage(int width, int height, TextureFormat format, bool inverse)
	{
		return GenerateNoiseImage(width, height, format, true, inverse);
	}
	public Texture2D GenerateNoiseImage(int layer, int width, int height, TextureFormat format, bool inverse)
	{
		return GenerateNoiseImage(layer, width, height, format, true, inverse);
	}

	public Texture2D GenerateNoiseImage(int width, int height, TextureFormat format, bool mipmaps, bool inverse)
	{
		return GenerateNoiseImage(-1, width, height, format, mipmaps, inverse);
	}
	public Texture2D GenerateNoiseImage(int layer, int width, int height, TextureFormat format, bool mipmaps, bool inverse)
	{
		//create texture
		Texture2D tex = new Texture2D(width, height, format, mipmaps);
		tex.filterMode = FilterMode.Bilinear;
		//generate colour array
		Color[] colours = new Color[tex.width * tex.height];
		int index = 0;
		for (int i = 0; i < tex.height; i++)
		{
			float y = ((float)(i * 10)) / ((float)tex.height);
			for (int j = 0; j < tex.width; j++)
			{
				float x = ((float)(j * 10)) / ((float)tex.width);
				float channel_value = GenerateNoiseChannel(layer, new Vector2(x, y), inverse);
				colours[index].r = channel_value;
				colours[index].g = channel_value;
				colours[index].b = channel_value;
				index += 1;
			}
		}
		//fill texture
		tex.SetPixels(colours);
		tex.Apply(mipmaps);
		return tex;
	}

	private float GenerateNoiseChannel(int layer, Vector2 point, bool inverse)
	{
		//Sample channel value
		float channel;
		if (layer < 0 || layer >= _layers)
		{
			channel = SampleNoise(point);
		}
		else
		{
			channel = SampleLayer(layer, point);
		}
		channel = Mathf.Clamp01(channel);
		//invert value if required
		if (inverse)
		{
			channel = 1 - channel;
		}
		return channel;
	}
	#endregion

	#region Getters
	public int layers { get { return _layers; } }

	public int GetOctaves(int layer)
	{
		if (layer < 0 || layer >= _layers)
		{
			throw new System.ArgumentOutOfRangeException("layer");
		}
		return _octaves[layer];
	}
	public int GetInfluence(int layer)
	{
		if (layer < 0 || layer >= _layers)
		{
			throw new System.ArgumentOutOfRangeException("layer");
		}
		return _influence[layer];
	}
	public float GetPersistence(int layer)
	{
		if (layer < 0 || layer >= _layers)
		{
			throw new System.ArgumentOutOfRangeException("layer");
		}
		return _persistence[layer];
	}
	public Vector2 GetLacunarity(int layer)
	{
		if (layer < 0 || layer >= _layers)
		{
			throw new System.ArgumentOutOfRangeException("layer");
		}
		return _lacunarity[layer];
	}
	public Vector2 GetOffset(int layer)
	{
		if (layer < 0 || layer >= _layers)
		{
			throw new System.ArgumentOutOfRangeException("layer");
		}
		return _offset[layer];
	}
	public Vector2 GetScale(int layer)
	{
		if (layer < 0 || layer >= _layers)
		{
			throw new System.ArgumentOutOfRangeException("layer");
		}
		return _scale[layer];
	}
	#endregion

	#region Setters
	public void SetOctaves(int layer, int octaves)
	{
		if (layer < 0 || layer >= _layers)
		{
			throw new System.ArgumentOutOfRangeException("layer");
		}
		_octaves[layer] = Mathf.Max(1, octaves);
	}
	public void SetInfluence(int layer, int influence)
	{
		if (layer < 0 || layer >= _layers)
		{
			throw new System.ArgumentOutOfRangeException("layer");
		}
		_influence[layer] = Mathf.Max(0, influence);
		//recalculate total influence
		_total_influence = CalculateTotalInfluence();
	}
	public void SetPersistence(int layer, float persistence)
	{
		if (layer < 0 || layer >= _layers)
		{
			throw new System.ArgumentOutOfRangeException("layer");
		}
		_persistence[layer] = Mathf.Clamp01(persistence);
	}
	public void SetLacunarity(int layer, Vector2 lacunarity)
	{
		if (layer < 0 || layer >= _layers)
		{
			throw new System.ArgumentOutOfRangeException("layer");
		}
		_lacunarity[layer] = lacunarity;
	}
	public void SetOffset(int layer, Vector2 offset)
	{
		if (layer < 0 || layer >= _layers)
		{
			throw new System.ArgumentOutOfRangeException("layer");
		}
		_offset[layer] = offset;
	}
	public void SetScale(int layer, Vector2 scale)
	{
		if (layer < 0 || layer >= _layers)
		{
			throw new System.ArgumentOutOfRangeException("layer");
		}
		if (scale.x == 0 || scale.y == 0)
		{
			throw new System.ArgumentOutOfRangeException("scale");
		}
		_scale[layer] = scale;
	}
	#endregion

	#region Default Values
	private int _default_octaves { get { return 1; } }
	private int _default_influence { get { return 1; } }
	private float _default_persistence { get { return 1; } }
	private Vector2 _default_lacunarity { get { return Vector2.one; } }
	private Vector2 _default_offset { get { return Vector2.zero; } }
	private Vector2 _default_scale { get { return Vector2.one; } }
	#endregion

	#region Helper Methods
	private float CalculateTotalInfluence()
	{
		float total = 0f;
		for (int k = 0;k < _layers;k++)
		{
			total += _influence[k];
		}
		return total;
	}
	#endregion
}
