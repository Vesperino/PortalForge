# Konfiguracja Supabase dla PortalForge

## 🔧 Wymagana konfiguracja Supabase Dashboard

### 1. URL Configuration

Przejdź do: **Authentication** → **URL Configuration**

#### Site URL
Ustaw główny URL Twojej aplikacji frontend:
```
https://krablab.pl/portalforge/fe
```
Lub dla developmentu:
```
http://localhost:3000
```

#### Redirect URLs
Dodaj następujące URLe (jeden na linię):
```
https://krablab.pl/portalforge/fe/auth/callback
https://krablab.pl/portalforge/fe/**
http://localhost:3000/auth/callback
http://localhost:3000/**
```

### 2. Email Templates

Przejdź do: **Authentication** → **Email Templates**

#### Confirm signup (Potwierdzenie rejestracji)

**Subject:**
```
Potwierdź swoje konto
```

**Message body:**
```html
<h2>Witaj w PortalForge!</h2>

<p>Dziękujemy za rejestrację. Aby aktywować swoje konto, kliknij w poniższy link:</p>

<p><a href="{{ .ConfirmationURL }}">Potwierdź adres email</a></p>

<p>Jeśli nie zakładałeś konta w naszym serwisie, zignoruj tę wiadomość.</p>

<p>Link weryfikacyjny jest ważny przez 24 godziny.</p>

<p>Pozdrawiamy,<br>
Zespół PortalForge</p>
```

**⚠️ WAŻNE:** W szablonie email NIE trzeba ręcznie ustawiać redirect_to - backend robi to automatycznie!

#### Magic Link

**Subject:**
```
Twój link logowania do PortalForge
```

**Message body:**
```html
<h2>Zaloguj się bez hasła</h2>

<p>Kliknij w poniższy link, aby zalogować się do swojego konta:</p>

<p><a href="{{ .ConfirmationURL }}">Zaloguj się</a></p>

<p>Link jest jednorazowy i wygasa po 60 minutach.</p>

<p>Jeśli nie prosiłeś o ten link, zignoruj tę wiadomość. Twoje konto pozostaje bezpieczne.</p>

<p>Pozdrawiamy,<br>
Zespół PortalForge</p>
```

#### Reset Password

**Subject:**
```
Resetowanie hasła do konta
```

**Message body:**
```html
<h2>Zresetuj swoje hasło</h2>

<p>Otrzymaliśmy prośbę o zresetowanie hasła do Twojego konta.</p>

<p>Aby ustawić nowe hasło, kliknij w poniższy link:</p>

<p><a href="{{ .ConfirmationURL }}">Zresetuj hasło</a></p>

<p>Jeśli nie prosiłeś o reset hasła, zignoruj tę wiadomość. Twoje hasło nie zostanie zmienione.</p>

<p>Link jest ważny przez 60 minut.</p>

<p>Pozdrawiamy,<br>
Zespół PortalForge</p>
```

#### Change Email Address

**Subject:**
```
Potwierdź zmianę adresu email
```

**Message body:**
```html
<h2>Zmiana adresu email</h2>

<p>Otrzymaliśmy prośbę o zmianę adresu email powiązanego z Twoim kontem.</p>

<p>Aby potwierdzić zmianę na nowy adres email, kliknij w poniższy link:</p>

<p><a href="{{ .ConfirmationURL }}">Potwierdź nowy adres email</a></p>

<p>Jeśli nie zmieniałeś adresu email, skontaktuj się z administratorem natychmiast.</p>

<p>Link weryfikacyjny wygasa po 24 godzinach.</p>

<p>Pozdrawiamy,<br>
Zespół PortalForge</p>
```

### 3. SMTP Settings

Przejdź do: **Project Settings** → **Auth** → **SMTP Settings**

Jeśli chcesz używać własnego SMTP (opcjonalne):
- Enable Custom SMTP: **ON**
- Sender email: twój email
- Sender name: **PortalForge**
- Host, Port, Username, Password: według Twojego dostawcy SMTP

**Lub zostaw domyślne ustawienia Supabase** - wysyłka emaili będzie działać automatycznie.

### 4. Email Rate Limits

Przejdź do: **Authentication** → **Rate Limits**

Zalecane ustawienia:
- **Email sending**: 4 requests per hour per email
- **Phone verification**: 4 requests per hour per phone (jeśli używasz)
- **Password reset**: 4 requests per hour per email

## 🔐 Zmienne środowiskowe Backend

W pliku `backend/PortalForge.Api/.env` ustaw:

```bash
# Frontend URL - MUSI pasować do Site URL w Supabase!
AppSettings__FrontendUrl=https://krablab.pl/portalforge/fe

# Lub dla developmentu:
AppSettings__FrontendUrl=http://localhost:3000
```

## ✅ Weryfikacja konfiguracji

### Test 1: Rejestracja użytkownika
1. Zarejestruj się przez frontend
2. Sprawdź czy dostałeś email
3. Kliknij w link weryfikacyjny
4. Powinieneś zostać przekierowany na `{FRONTEND_URL}/auth/callback`
5. Zobaczysz komunikat "Konto zostało aktywowane!"
6. Po 3 sekundach zostaniesz przekierowany na stronę główną z kalendarzem

### Test 2: Logi backendu
Sprawdź logi backend czy widzisz:
```
Registering user with redirect URL: https://krablab.pl/portalforge/fe/auth/callback
User registered successfully: user@example.com. Supabase will send verification email.
```

### Test 3: Format linku w emailu
Link w emailu powinien wyglądać tak:
```
https://mqowlgphivdosieakzjb.supabase.co/auth/v1/verify?
  token=XXX&
  type=signup&
  redirect_to=https://krablab.pl/portalforge/fe/auth/callback
```

**Zwróć uwagę na `redirect_to`** - powinien wskazywać na Twój frontend!

### Test 4: Format przekierowania
Po kliknięciu w link, Supabase automatycznie przekierowuje na `{redirect_to}` z tokenami w hash:
```
https://krablab.pl/portalforge/fe/auth/callback#access_token=XXX&refresh_token=YYY&expires_in=3600&token_type=bearer&type=signup
```

Frontend callback wyciąga tokeny z hash i weryfikuje przez backend.

## 🐛 Rozwiązywanie problemów

### Problem: Email nie zawiera redirect_to lub wskazuje zły URL

**Rozwiązanie:**
1. Upewnij się, że `AppSettings__FrontendUrl` w `.env` jest poprawny
2. Zrestartuj backend
3. Zarejestruj nowego użytkownika (stary email już nie zadziała)
4. Sprawdź logi czy widzisz prawidłowy redirect URL

### Problem: 303 See Other po kliknięciu w link

**Rozwiązanie:**
1. Sprawdź czy URL w **Redirect URLs** zawiera `{FRONTEND_URL}/auth/callback`
2. Upewnij się że używasz `https://` dla produkcji i `http://` dla localhost
3. Dodaj wildcard: `{FRONTEND_URL}/**`

### Problem: "Invalid redirect URL" w Supabase

**Rozwiązanie:**
1. Sprawdź czy redirect URL jest dokładnie taki sam w:
   - Backend `.env` (`AppSettings__FrontendUrl`)
   - Supabase Dashboard → URL Configuration → Redirect URLs
2. Nie zapomnij dodać `/auth/callback` na końcu!

### Problem: Strona callback zwraca błąd

**Rozwiązanie:**
1. Sprawdź plik [frontend/pages/auth/callback.vue](frontend/pages/auth/callback.vue)
2. Upewnij się że frontend jest uruchomiony
3. Sprawdź console w przeglądarce (F12) czy są błędy JavaScript

## 📝 Notatki

- Link weryfikacyjny jest jednorazowy - nie można go użyć dwukrotnie
- Link wygasa po 24 godzinach
- Backend automatycznie dodaje `/auth/callback` do frontend URL
- Supabase automatycznie wysyła email - nie trzeba konfigurować custom SMTP (chyba że chcesz)
- Użytkownicy z niezweryfikowanym emailem mogą się re-rejestrować - stare konto zostanie usunięte

## 🔗 Przydatne linki

- [Supabase Auth Docs](https://supabase.com/docs/guides/auth)
- [Email Templates Guide](https://supabase.com/docs/guides/auth/auth-email-templates)
- [Redirect URLs Configuration](https://supabase.com/docs/guides/auth/redirect-urls)
