using System;
using Engine;
using Engine.Input;
using Engine.Universe;
using Engine.Rendering;
using Engine.Modding;
using Engine.Resources;
using Engine.Scripting;
using Common.Cameras;

namespace UpvoidMiner
{
	public class UpvoidMinerNetworkWorldReceiver : SimpleWorldGenerator
	{
		TerrainMaterial MatGround;

		/// <summary>
		/// Initializes the terrain materials and settings.
		/// </summary>
		public override bool init()
		{
			return base.init();
		}

		/// <summary>
		/// Creates the CSG node network for the terrain generation.
		/// </returns>
		public override CsgNode createTerrain()
		{
			// Load and return a CsgNode based on the "Hills" expression resource. This will create some generic perlin-based hills.
			CsgOpUnion union = new CsgOpUnion();
			return union;
		}

	}


	/// <summary>
	/// Main class for the local scripting domain.
	/// </summary>
	public class LocalScript
	{
		public static GenericCamera Camera;
		public static FreeCameraControl CameraControl;

		public static Module Mod;
		public static ResourceDomain ModDomain;

		/// <summary>
		/// Updates the camera position.
		/// </summary>
        private static void Update(float _elapsedSeconds)
        {
            CameraControl.Update(_elapsedSeconds);
        }

		/// <summary>
		/// Initializes the local part of the mod.
		/// </summary>
		public static void Startup (IntPtr _unmanagedModule)
		{
			if (!Scripting.IsHost)
			{
				Universe.CreateWorld("UpvoidMinerWorld", new UpvoidMinerNetworkWorldReceiver());
			}

			// Create a simple camera that allows free movement.
			Camera = new GenericCamera ();
			Camera.FarClippingPlane = 3500.0;
			CameraControl = new FreeCameraControl (-10f, Camera);

			// Get the world (created by the host script)
			World world = Universe.GetWorldByName ("UpvoidMinerWorld");

			// Place the camera in the world
			world.AttachCamera (Camera);
			if (Rendering.ActiveMainPipeline != null)
				Rendering.ActiveMainPipeline.SetCamera (Camera);

			GC.KeepAlive (Camera);

			if (!Scripting.IsHost) {
				ivec3 refPos = new ivec3 (0, 0, 0);
				float activeRadius = 10;
				float sleepingRadius = 20;
				float minLODRange = 10;
				float falloff = 5;
				world.AddActiveRegion (refPos, activeRadius, sleepingRadius, minLODRange, falloff);
			}

			// Configure the camera to receive user input
			Input.RootGroup.AddListener (CameraControl);

			// Registers the update callback that updates the camera position.
			Scripting.RegisterUpdateFunction(Update, 1 / 60.0f, 3 / 60.0f);
		}
	}
}
