// Guest session management utilities
const GUEST_SESSION_KEY = 'guest_session_id';

export const getGuestSessionId = (): string => {
  let sessionId = localStorage.getItem(GUEST_SESSION_KEY);

  if (!sessionId) {
    // Generate a unique session ID for guest users
    sessionId = `guest_${Date.now()}_${Math.random().toString(36).substring(2, 15)}`;
    localStorage.setItem(GUEST_SESSION_KEY, sessionId);
  }

  return sessionId;
};

export const clearGuestSession = (): void => {
  localStorage.removeItem(GUEST_SESSION_KEY);
};

export const hasGuestSession = (): boolean => {
  return !!localStorage.getItem(GUEST_SESSION_KEY);
};