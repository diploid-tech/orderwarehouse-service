#!/bin/sh

# Wait until istio-proxy is running before starting the application to prevent egress network connections to be blocked!
# See: https://github.com/istio/istio/issues/11130
until curl --head localhost:15000
do
	sleep 3
done

exec dotnet Avanti.OrderWarehouseService.dll
