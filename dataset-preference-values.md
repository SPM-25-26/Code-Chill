# Dataset preference vocabulary

Source: `src/backend/chill-code-backend/App_Data/chillcode.db` (SQLite) as of this snapshot.

## Events
- Typology: Festival; Fiera / Salone; Musical event; Patronal Day/Feast of Saints; Sagra; Show; Sports meeting; Theatrical performance.
- Audience: Adulti e bambini; Adults and children; grownups; Adulti

## Routes
- TravellingMethod: Camminare; Ciclismo.
- PathTheme: Archeologico; Culturale; Naturalistico; Religiosi; Storico.
- Duration: 10 minuti; 20 minuti; 30 minuti; 45 minuti; 50 minuti; 1 ora; 1 ora e 30 minuti; 2 ore; 3 ore; 4 ore; metà giornata; una giornata.
- RouteLength (km): 1; 2; 3; 4; 5; 6; 8; 9; 10; 15; 18; 26.
- SecurityLevel: no values stored (all rows null/empty).
- NumberOfStages: only 0 stored in current rows.
- QuantifiedPathwayPaving: only 0 stored in current rows.

## Articles
- Paragraph referenceCategory values: ArtCulture; EntertainmentLeisure; Events; Nature.

## POIs
- Categories: ArtCulture; EatAndDrink; EntertainmentLeisure; Nature; Services; Shopping; Sleep; TypicalProducts.
- Observed typologies in current snapshot: Alloggio in ambito di attività agrituristica; Belvedere; Caffetteria; Cantina Produttiva; Chiesa; Montagna; Monumento; Museo; Ristorante Tradizionale; Santuario; Torre.

## Current questionnaire (version v1)
- travelVibe (binary, required): chill; adventure.
- activityLevel (single, required): low; medium; high.
- citySize (binary, required): small; big.
- interests (multi, optional, 0–8): Cultural & Historical Sites; Nature & Outdoor Activities; Food & Wine Experiences; Festivals & Events; Art & Museums; Architecture; Local Traditions; Sports & Adventure; Religious Sites; Shopping & Markets.

## Proposed questionnaire-to-content mapping
- travelVibe:
  - chill → all Articles; Events with Typology in [Musical event, Theatrical performance].
  - adventure → Events with Typology in [Festival, Fiera / Salone, Patronal Day/Feast of Saints, Sagra, Show, Sports meeting].
- activityLevel (Routes by Duration):
  - low → Duration up to 1 ora (include 10–50 minuti and 1 ora, 1 ora e 30 minuti? choose ≤ 1h if strict).
  - medium → >1 ora to ≤4 ore (1 ora e 30 minuti, 2 ore, 3 ore, 4 ore).
  - high → metà giornata, una giornata.
- citySize:
  - Data missing in current content; skip in scoring until Municipality size metadata exists.
- audience (Events):
  - Adults and children / Adulti e bambini → flag as family-friendly; can boost when a future “family-friendly” preference exists. Adulti → adult-oriented; treat as neutral otherwise.
- route length (Routes):
  - Use as a fallback/refiner for activityLevel when Duration is missing or inconsistent. Suggested: low ≤5 km, medium 5–12 km, high >12 km.
- interests:
  - Nature & Outdoor Activities → Routes (any PathTheme, especially Naturalistico); Articles with referenceCategory Nature.
  - Food & Wine Experiences → Events with Typology Sagra or Fiera / Salone.
  - Festivals & Events → Events with Typology [Festival, Show, Patronal Day/Feast of Saints, Musical event, Theatrical performance, Sports meeting]; Articles with referenceCategory Events.
  - Art & Museums → Articles with referenceCategory ArtCulture.
  - Architecture → Routes with PathTheme Archeologico.
  - Local Traditions → Routes with PathTheme Culturale or Storico; Events with Typology Patronal Day/Feast of Saints.
  - Sports & Adventure → Events with Typology Sports meeting; Routes with TravellingMethod Ciclismo.
  - Religious Sites → Routes with PathTheme Religiosi.
  - Shopping & Markets → Events with Typology Fiera / Salone.
- POIs:
  - Category → interests: ArtCulture → Cultural & Historical Sites / Art & Museums / Architecture; Nature → Nature & Outdoor; EatAndDrink → Food & Wine; Shopping → Shopping & Markets (adventure vibe); EntertainmentLeisure → Festivals & Events (adventure vibe); TypicalProducts → Food & Wine / Shopping & Markets / Local Traditions.
  - Typology → interests: Museo → Art & Museums / Cultural & Historical Sites; Monumento/Torre → Cultural & Historical Sites / Architecture; Chiesa → Religious Sites / Architecture; Santuario → Religious Sites; Belvedere → Nature & Outdoor; Montagna → Nature & Outdoor / Sports & Adventure; Caffetteria/Ristorante Tradizionale → Food & Wine; Cantina Produttiva → Food & Wine / Shopping & Markets; Alloggio in ambito di attività agrituristica → Food & Wine / Nature & Outdoor / Local Traditions.

## Scoring logic (frontend sorting)
- Base: scores start at 0; feed is never filtered out, only ordered.
- travelVibe: chill boosts Musical/Theatrical events and low-effort routes; adventure boosts Festival/Fiera/Sagra/Patronal/Show/Sports events, medium/high routes/biking, and POI categories Nature/EntertainmentLeisure/Shopping.
- activityLevel: derived from duration (fallback length) → low/medium/high; routes score +1 on exact match, +0.5 on adjacent, else 0.
- familyFriendly: Yes adds +0.3 to family-friendly events; No subtracts 0.2 from family-friendly events.
- interests:
  - Events: typology → buckets as above; each match +1.
  - Routes: pathTheme → buckets as above; TravellingMethod Ciclismo → Sports & Adventure (+1). Nature & Outdoor adds a small +0.3 universal outdoor bump.
  - Articles: paragraph referenceCategory → interests (Nature→Nature; Events→Festivals & Events; ArtCulture→Art & Museums; EntertainmentLeisure→Festivals & Events/Local Traditions). Chill vibe adds +0.3. For adventure or high-activity users, article scores are capped at 1.0 total.
  - POIs: category/typology buckets as above; matches +1; adventure vibe applies to Nature/EntertainmentLeisure/Shopping.
- Keyword fallback: titles/descriptions/themes tokenized; each interest keyword hit adds up to +0.3.
- Frontend filtering: “recommended” is score ≥ 0.5; “might like” is < 0.5.
