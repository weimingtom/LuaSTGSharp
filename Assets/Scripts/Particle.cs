﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class ResParticle : Resource
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ParticleInfo
	{
		public uint BlendInfo;

		public int Emission;  // 每秒发射个数
		public float Lifetime;  // 生命期
		public float ParticleLifeMin;  // 粒子最小生命期
		public float ParticleLifeMax;  // 粒子最大生命期
		public float Direction;  // 发射方向
		public float Spread;  // 偏移角度
		[MarshalAs(UnmanagedType.Bool)]
		public bool Relative;  // 使用相对值还是绝对值

		public float SpeedMin;  // 速度最小值
		public float SpeedMax;  // 速度最大值

		public float GravityMin;  // 重力最小值
		public float GravityMax;  // 重力最大值

		public float RadialAccelMin;  // 最低线加速度
		public float RadialAccelMax;  // 最高线加速度

		public float TangentialAccelMin;  // 最低角加速度
		public float TangentialAccelMax;  // 最高角加速度

		public float SizeStart;  // 起始大小
		public float SizeEnd;  // 最终大小
		public float SizeVar;  // 大小抖动值

		public float SpinStart;  // 起始自旋
		public float SpinEnd;  // 最终自旋
		public float SpinVar;  // 自旋抖动值

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public float[] ColorStart;  // 起始颜色(rgba)
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public float[] ColorEnd;  // 最终颜色
		public float ColorVar;  // 颜色抖动值
		public float AlphaVar;  // alpha抖动值
	}

	private ParticleInfo _particleInfo;
	private bool _loaded;
	private Material _material;
	public Vector2 Ab { get; set; }
	public bool Rect{ get; set; }
		
	public ResParticle(string name)
		: base(name)
	{
	}

	public override bool InitFromStream(Stream stream)
	{
		if (_loaded)
		{
			return false;
		}

		using (var memStream = new MemoryStream())
		{
			stream.CopyTo(memStream);

			var gcHandle = GCHandle.Alloc(memStream.ToArray(), GCHandleType.Pinned);
			_particleInfo = (ParticleInfo) Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(ParticleInfo));
			_particleInfo.ParticleLifeMin /= 10;
			_particleInfo.ParticleLifeMax /= 10;
			gcHandle.Free();
		}
		
		_loaded = true;
		return true;
	}

	public void SetMaterial(Material material)
	{
		if (_material != null)
		{
			throw new ResourceException(typeof(ResParticle), "Material has been set.");
		}

		_material = material;
	}

	public void SetupParticleSystem(ParticleSystem particleSystem)
	{
		if (!_loaded || _material == null)
		{
			return;
		}

		if (particleSystem == null)
		{
			return;
		}
		
		particleSystem.Stop();

		var main = particleSystem.main;
		var emission = particleSystem.emission;
		var colorOverLifetime = particleSystem.colorOverLifetime;
		var renderer = particleSystem.GetComponent<Renderer>();

		renderer.material = _material;

		// BlendInfo unknown
		emission.rateOverTime = _particleInfo.Emission;
		if (_particleInfo.Lifetime > 0)
		{
			main.duration = _particleInfo.Lifetime;
		}
		main.startLifetime = new ParticleSystem.MinMaxCurve(_particleInfo.ParticleLifeMin, _particleInfo.ParticleLifeMax);
		main.startRotation = _particleInfo.Direction;
		main.randomizeRotationDirection = _particleInfo.Spread;
		// Relative unknown
		main.startSpeed = new ParticleSystem.MinMaxCurve(_particleInfo.SpeedMin, _particleInfo.SpeedMax);
		main.gravityModifier = new ParticleSystem.MinMaxCurve(_particleInfo.GravityMin, _particleInfo.GravityMax);
		// RadialAccel unknown
		// TangentialAccel unknown
		main.startSize = _particleInfo.SizeStart;
		// Spin unknown
		main.startColor = new Color(_particleInfo.ColorStart[0], _particleInfo.ColorStart[1], _particleInfo.ColorStart[2],
					_particleInfo.ColorStart[3]);
		colorOverLifetime.color = new ParticleSystem.MinMaxGradient(
				new Color(_particleInfo.ColorStart[0], _particleInfo.ColorStart[1], _particleInfo.ColorStart[2],
					_particleInfo.ColorStart[3]),
				new Color(_particleInfo.ColorEnd[0], _particleInfo.ColorEnd[1], _particleInfo.ColorEnd[2],
					_particleInfo.ColorEnd[3]));
		// Var unknown
		
		particleSystem.Play();
	}
}
