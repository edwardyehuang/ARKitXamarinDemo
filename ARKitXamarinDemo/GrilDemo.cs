using System;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Urho.Navigation;

namespace ARKitXamarinDemo
{
    public class GrilDemo : ArkitApp
    {
		protected bool surfaceIsValid;
		protected bool positionIsSelected;

        protected Node cursorNode;
        protected StaticModel cursorModel;

        protected Text loadingLabel;

        protected DynamicNavigationMesh navMesh;
        protected CrowdManager crowdManager;

        protected Node _girlNode;
        protected const string girlModel = "";

        [Preserve]
        public GrilDemo(Urho.ApplicationOptions opts) : base(opts)
        {
            
        }

        protected override void Start()
        {
            base.Start();

            Scene.CreateComponent<DebugRenderer>();

			cursorNode = Scene.CreateChild();
			cursorNode.Position = Vector3.UnitZ * 100; //hide cursor at start - pos at (0,0,100) 

			cursorModel = cursorNode.CreateComponent<Urho.Shapes.Plane>();
			cursorModel.ViewMask = 0x80000000; //hide from raycasts (Raycast() uses a differen viewmask so the cursor won't be visible for it)

			cursorNode.RunActions(new RepeatForever(new ScaleTo(0.3f, 0.15f), new ScaleTo(0.3f, 0.2f)));

			var cursorMaterial = new Material();
			cursorMaterial.SetTexture(TextureUnit.Diffuse, ResourceCache.GetTexture2D("Textures/Cursor.png"));
			cursorMaterial.SetTechnique(0, CoreAssets.Techniques.DiffAlpha);
			cursorModel.Material = cursorMaterial;
			cursorModel.CastShadows = false;

			Input.TouchEnd += args => OnGestureTapped(args.X, args.Y);
			UnhandledException += OnUnhandledException;

            ContinuesHitTestAtCenter = true;

			loadingLabel = new Text
			{
				Value = "Detecting planes...",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				TextAlignment = HorizontalAlignment.Center,
			};

			loadingLabel.SetColor(new Color(0.5f, 1f, 0f));
			loadingLabel.SetFont(font: CoreAssets.Fonts.AnonymousPro, size: 42);
			UI.Root.AddChild(loadingLabel);
		}

        protected void OnGestureTapped(int argsX, int argsY)
        {
            if (surfaceIsValid && !positionIsSelected)
            {
                UI.Root.RemoveChild(loadingLabel);
                loadingLabel = null;
                PlaneDetectionEnabled = false;

                ContinuesHitTestAtCenter = false;
                positionIsSelected = true;

				var hitPos = cursorNode.Position;// - Vector3.UnitZ * 0.01f;
				

				navMesh = Scene.CreateComponent<DynamicNavigationMesh>();
				Scene.CreateComponent<Navigable>();

				navMesh.CellSize = 0.01f;
				navMesh.CellHeight = 0.05f;
				navMesh.DrawOffMeshConnections = true;
				navMesh.DrawNavAreas = true;
				navMesh.TileSize = 1;
				navMesh.AgentRadius = 0.1f;

				navMesh.Build();

				crowdManager = Scene.CreateComponent<CrowdManager>();
				var parameters = crowdManager.GetObstacleAvoidanceParams(0);
				parameters.VelBias = 0.5f;
				parameters.AdaptiveDivs = 7;
				parameters.AdaptiveRings = 3;
				parameters.AdaptiveDepth = 3;
				crowdManager.SetObstacleAvoidanceParams(0, parameters);

            }
        }

        protected void InitGirlNode (Vector3 pos, string name = "girl")
        {
            _girlNode = Scene.CreateChild();
            _girlNode.Position = pos;

            var modelObject = _girlNode.CreateComponent<AnimatedModel>();

			modelObject.CastShadows = true;
			modelObject.Model = ResourceCache.GetModel(girlModel);
			modelObject.SetMaterial(ResourceCache.GetMaterial(girlModel).Clone());
			modelObject.Material.SetTechnique(0, ResourceCache.GetTechnique("Techniques/DiffOutline.xml"));
			HighlightMaterial(modelObject.Material, false);

			var shadowPlaneNode = _girlNode.CreateChild();
			shadowPlaneNode.Scale = new Vector3(10, 1, 10);
			shadowPlaneNode.CreateComponent<Urho.SharpReality.TransparentPlaneWithShadows>();

			var agent = _girlNode.CreateComponent<CrowdAgent>();
			agent.Height = 0.2f;
			agent.NavigationPushiness = NavigationPushiness.Medium;
			agent.MaxSpeed = 0.6f;
			agent.MaxAccel = 0.6f;
			agent.Radius = 0.06f;
			agent.NavigationQuality = NavigationQuality.Medium;
        }

		protected void HighlightMaterial(Material material, bool higlight)
		{
			material.SetShaderParameter("OutlineColor", higlight ? new Color(1f, 0.75f, 0, 0.5f) : Color.Transparent);
			material.SetShaderParameter("OutlineWidth", higlight ? 0.009f : 0f);
		}

		protected void OnUnhandledException(object sender, Urho.UnhandledExceptionEventArgs e)
		{
			e.Handled = true;
			System.Console.WriteLine(e);
		}
	}

}
