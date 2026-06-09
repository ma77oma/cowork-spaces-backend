const baseUrl = process.env.API_BASE_URL ?? 'http://localhost:5254';
const spaceId = process.env.SPACE_ID ?? '11111111-1111-1111-1111-111111111111';
const email = process.env.TEST_EMAIL ?? `loadtest_${Date.now()}@coworkspaces.local`;
const password = process.env.TEST_PASSWORD ?? 'Test1234';

const tomorrow = new Date();
tomorrow.setDate(tomorrow.getDate() + 1);
tomorrow.setHours(10, 0, 0, 0);

const end = new Date(tomorrow);
end.setHours(12, 0, 0, 0);

const payload = {
  spaceId,
  startAt: tomorrow.toISOString(),
  endAt: end.toISOString()
};

async function createReservation(label) {
  const token = await getToken();
  const response = await fetch(`${baseUrl}/api/reservations`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`
    },
    body: JSON.stringify(payload)
  });

  const text = await response.text();
  return { label, status: response.status, body: text };
}

let cachedToken;

async function getToken() {
  if (cachedToken) {
    return cachedToken;
  }

  const registerResponse = await fetch(`${baseUrl}/api/auth/register`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      fullName: 'Concurrency Test User',
      email,
      password
    })
  });

  if (registerResponse.ok) {
    const body = await registerResponse.json();
    cachedToken = body.token;
    return cachedToken;
  }

  const loginResponse = await fetch(`${baseUrl}/api/auth/login`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ email, password })
  });

  if (!loginResponse.ok) {
    throw new Error(`Unable to authenticate test user. Status ${loginResponse.status}`);
  }

  const loginBody = await loginResponse.json();
  cachedToken = loginBody.token;
  return cachedToken;
}

async function main() {
  console.log('Payload:', payload);

  const [first, second] = await Promise.all([
    createReservation('Request 1'),
    createReservation('Request 2')
  ]);

  console.log(`${first.label}: ${first.status}`);
  console.log(first.body);
  console.log(`${second.label}: ${second.status}`);
  console.log(second.body);
  console.log('Expected: one 201 Created and one 409 Conflict.');
}

main().catch((error) => {
  console.error('Concurrency test failed:', error);
  process.exitCode = 1;
});
