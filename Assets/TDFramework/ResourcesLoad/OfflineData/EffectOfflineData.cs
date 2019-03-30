using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDFramework
{
    public class EffectOfflineData : OfflineData
    {

		public ParticleSystem[] m_particleSystem;
		public TrailRenderer[] m_trailRenderer;

		public override void ResetProperty()
		{
			base.ResetProperty();
			int particleCount = m_particleSystem.Length;
			for(int i = 0; i < particleCount; ++i)
			{
				m_particleSystem[i].Clear(true);
				m_particleSystem[i].Play();
			}
			int trailRendererCount = m_trailRenderer.Length;
			for(int i = 0; i < trailRendererCount; ++i)
			{
				m_trailRenderer[i].Clear();
			}
		}
		public override void BindData()
		{
			base.BindData();
			m_particleSystem = gameObject.GetComponentsInChildren<ParticleSystem>(true);
			m_trailRenderer = gameObject.GetComponentsInChildren<TrailRenderer>(true);

		}
    }
}
