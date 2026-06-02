using Core.Dependency;

namespace Shapez2Multiplayer
{
    public class HUDMultiplayerMassSelectionsHost : HUDPart
    {
        public static HUDMultiplayerMassSelectionsHost Instance { get; private set; }
        [Construct]
        private void Construct(IBuildingPlacementIndicatorAccessor buildingPlacementIndicators, ITutorialHighlightProvider tutorialHighlightProvider, IMapModel map, IIslandPreviewDrawer islandPreviewDrawer)
        {
            BuildingPlacementIndicators = buildingPlacementIndicators;
            TutorialHighlightProvider = tutorialHighlightProvider;
            Map = map;
            IslandPreviewDrawer = islandPreviewDrawer;
            Instance = this;
        }
        public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
        {
            if (MultiplayerCore.Client)
            {
                MultiplayerCore.connectionManager.HostBuildingMassSelection?.OnGameUpdate(context, drawOptions);
                MultiplayerCore.connectionManager.HostIslandMassSelection?.OnGameUpdate(context, drawOptions);
                foreach (var buildingMassSelection in MultiplayerCore.connectionManager.PlayersBuildingMassSelections.Values)
                {
                    buildingMassSelection.OnGameUpdate(context, drawOptions);
                }
                foreach (var islandMassSelection in MultiplayerCore.connectionManager.PlayersIslandMassSelections.Values)
                {
                    islandMassSelection.OnGameUpdate(context, drawOptions);
                }
            }
            if (MultiplayerCore.Hosting)
            {
                foreach (var buildingMassSelection in MultiplayerCore.socketManager.PlayersBuildingMassSelections.Values)
                {
                    buildingMassSelection.OnGameUpdate(context, drawOptions);
                }
                foreach (var islandMassSelection in MultiplayerCore.socketManager.PlayersIslandMassSelections.Values)
                {
                    islandMassSelection.OnGameUpdate(context, drawOptions);
                }
            }
        }
        public OtherPlayerHUDBuildingMassSelection CreateOtherPlayerHUDBuildingMassSelection(IConnection? connection)
        {
            return new OtherPlayerHUDBuildingMassSelection(Player, BuildingPlacementIndicators, TutorialHighlightProvider, connection);
        }
        public OtherPlayerHUDIslandMassSelection CreateOtherPlayerHUDIslandMassSelection(IConnection? connection)
        {
            return new OtherPlayerHUDIslandMassSelection(Map, IslandPreviewDrawer, connection);
        }

        protected override void OnDispose()
        {
            Instance = null;
        }

        private IBuildingPlacementIndicatorAccessor BuildingPlacementIndicators;
        private ITutorialHighlightProvider TutorialHighlightProvider;
        private IMapModel Map;
        private IIslandPreviewDrawer IslandPreviewDrawer;
    }
}
