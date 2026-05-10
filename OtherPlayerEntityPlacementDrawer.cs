using Core.Logging;
using Game.Placement.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Unity.Core.Profiling;

namespace Shapez2Multiplayer
{
    public class OtherPlayerEntityPlacementDrawer
    {
        public bool HasData = false;
        public OtherPlayerEntityPlacementDrawer(IEnumerable<IPlacementDrawer> placementDrawers, ILogger logger)
        {
            this.Logger = logger;
            this.PlacementDrawers = placementDrawers.ToArray<IPlacementDrawer>();
        }
        public void Draw(FrameDrawOptions drawOptions, IMapModel map)
        {
            using (new ScopedProfilerSample("EntityPlacementDrawer.Draw"))
            {
                foreach (IPlacementDrawer placementDrawer in this.PlacementDrawers)
                {
                    try
                    {
                        placementDrawer.Draw(drawOptions);
                    }
                    catch (Exception ex)
                    {
                        ILogChannel exception = this.Logger.Exception;
                        if (exception != null)
                        {
                            exception.LogException(ex);
                        }
                    }
                }
            }
        }

        public void OnPlacementDataChanged([DisallowNull] IPlacementData placementData, PlacementInputHolder placementInput)
        {
            if (placementData == null)
            {
                throw new ArgumentNullException("placementData");
            }
            this.UpdatePlacementDataForAllDrawers(placementData, placementInput);
        }

        private void UpdatePlacementDataForAllDrawers(IPlacementData placementData, PlacementInputHolder placementInput)
        {
            using (new ScopedProfilerSample("EntityPlacementDrawer.UpdatePlacementDataForAllDrawers"))
            {
                foreach (IPlacementDrawer placementDrawer in this.PlacementDrawers)
                {
                    using (new ScopedProfilerSample(placementDrawer.GetType().Name ?? ""))
                    {
                        try
                        {
                            placementDrawer.SetNewPlacementData(placementData, placementInput);
                        }
                        catch (Exception ex)
                        {
                            ILogChannel exception = this.Logger.Exception;
                            if (exception != null)
                            {
                                exception.LogException(ex);
                            }
                        }
                    }
                }
            }
        }

        private readonly ILogger Logger;

        private readonly IPlacementDrawer[] PlacementDrawers;
    }
}
