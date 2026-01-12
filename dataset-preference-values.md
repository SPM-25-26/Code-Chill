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

## Scoring logic (frontend sorting)
- Base: everything stays in the feed; scores only influence ordering.
- travelVibe: chill biases Musical/Theatrical events and low-effort routes; adventure biases other event typologies and higher-effort routes/biking.
- activityLevel: derived from duration (fallback length) → low/medium/high; matches give strongest boost, adjacent levels smaller.
- familyFriendly: boosts family-friendly audiences when set to Yes; “Doesn’t matter” keeps it neutral; No gently penalizes family-friendly events (sort lower).
- interests:
  - Events: typology → interest buckets (Sagra/Fiera → Food & Wine/Shopping; Festival/Show/Patronal/Musical/Theatrical/Sports meeting → Festivals & Events; Sports meeting → Sports & Adventure; Patronal → Local Traditions).
  - Routes: pathTheme → interest buckets (Archeologico→Architecture; Culturale/Storico→Local Traditions; Naturalistico→Nature; Religiosi→Religious Sites); TravellingMethod Ciclismo → Sports & Adventure; all routes get a small Nature & Outdoor boost when selected.
  - Articles: paragraph referenceCategory → interests (Nature→Nature; Events→Festivals & Events; ArtCulture→Art & Museums; EntertainmentLeisure→Festivals & Events/Local Traditions); title/themes keyword fallback.
- Keyword fallback: if no structured match, titles/descriptions/themes are tokenized against interest keywords for a small boost.
