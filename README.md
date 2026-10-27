# Kiroldegiaren APIa

APIaren biltegia da hau. C# interfazea alboko `softDeleteEntrega` karpetan eta beste Git biltegi batean dago.
Lan honen aldaketak **`softDelete` adarrean** egiten dira.

## Bakoitzaren datu-baseko konexioa

1. Kopiatu `ApiErreserba/appsettings.Local.example.json` fitxategia karpeta berean, `appsettings.Local.json` izenarekin.
2. Fitxategi lokalean jarri zure MySQL zerbitzaria, datu-basea, erabiltzailea eta pasahitza.
3. Abiarazi APIa `ApiErreserba` karpetatik: `dotnet run --launch-profile http`.

`appsettings.Local.json` Git-ek baztertzen du. Ez gehitu eskuz commit batean.
`appsettings.json` partekatuak ez du bakoitzaren pasahitzik behar.
Tokiko konfigurazioaren gainetik ingurune-aldagaiek eta abiarazteko argumentuek dute lehentasuna.
Ingurune-aldagaia erabiltzeko gakoa `ConnectionStrings__Default` da.

APIaren berezko helbidea **`http://localhost:5289`** da, profilik gabe abiarazita ere.
Bi aplikazioak probatzeko, APIak martxan egon behar du interfazea irekitzean.

## Dagoeneko erabilgarri

| Eragiketa | Eskaera |
| --- | --- |
| Bazkidearen IDarekin saioa hasi | `POST /api/Bazkidea/login`, gorputza: `{"id":1}` |
| Pista eta haren erreserbak desgaitu | `PATCH /api/Pista/{id}/desgaitu` |
| Pista eta haren erreserbak berreskuratu | `PATCH /api/Pista/{id}/berreskuratu` |

Zerrenden SELECT, erreserben INSERT eta behin betiko DELETE funtzioak taldekideek osatuko dituzte.
Lehendik dagoen `ApiErreserba.http` fitxategiko adibidea bere horretan mantentzen da.

## Interfazea konektatzeko kontratua

Erabili `Responses/ApiResponse.cs` eta `Responses/ApiError.cs` erantzunetarako.
Zerrendek `status: "success"`, `type: "list"` eta `dataList: [...]` behar dituzte.
Elementu bakar batek `type: "element"` eta `data: {...}` erabiltzen ditu.
Erroreek dagokien HTTP kodea eta `message` euskaraz itzuli behar dituzte.
Behin betiko DELETE eragiketak arrakasta-erantzun hori edo HTTP 204 itzul dezake.

Erreserbetan, `data` balioaren formatua `yyyy-MM-dd` da, eta `ordua` balioarena `HH:mm:ss`.
Interfazeko lankidetza-gida alboko biltegiko `TALDEKIDEENTZAT.md` fitxategian dago.
