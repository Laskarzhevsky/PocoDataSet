CREATE PROCEDURE [HostedApplicationLayer].[FindHostedApplicationLayersByIdentifiers]
    @HostedApplicationLayer dbo.HostedApplicationLayer READONLY
AS
BEGIN
    SELECT H.*
    FROM HostedApplicationLayer H
    WHERE H.IsArchived = 0
      AND H.IsDeleted = 0
      AND EXISTS
      (
          SELECT 1
          FROM @HostedApplicationLayer I
          WHERE I.DomainName = H.DomainName
            AND I.UseCaseName = H.UseCaseName
            AND I.ApplicationLayerName = H.ApplicationLayerName
      )
END