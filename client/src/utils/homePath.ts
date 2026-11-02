/** Post-login / catch-all home: managers land on Tailwind admin dashboard. */
export function homePath(isManager: boolean): string {
  return isManager ? '/dashboard' : '/applications'
}
