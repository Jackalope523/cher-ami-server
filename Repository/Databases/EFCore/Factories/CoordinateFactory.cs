using NetTopologySuite.Geometries;

namespace Repository
{
    internal class CoordinateFactory : Factory
    {
        private readonly GeometryFactory internalFactory;

        internal CoordinateFactory()
        {
            NetTopologySuite.NtsGeometryServices.Instance = new NetTopologySuite.NtsGeometryServices
                (
                    NetTopologySuite.Geometries.Implementation.CoordinateArraySequenceFactory.Instance,
                    new PrecisionModel(1000d),
                    4326,   
                    GeometryOverlay.NG,
                    new CoordinateEqualityComparer()
                );

            internalFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        }

        internal Point Create(double longitude, double latitude)
        {         
            return internalFactory.CreatePoint(new Coordinate(longitude, latitude));
        }
    }
}
