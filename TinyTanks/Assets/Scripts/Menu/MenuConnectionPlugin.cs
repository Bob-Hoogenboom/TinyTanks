using UnityEngine;
using Fusion.Menu;

namespace SimpleFPS
{
	public abstract class MenuConnectionPlugin : MonoBehaviour
	{
		public abstract IFusionMenuConnection Create(MenuConnectionBehaviour connectionBehaviour);
	}
}
