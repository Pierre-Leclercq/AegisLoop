import { render, screen, within } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import App from '../App';

describe('App', () => {
  it('renders without crashing', () => {
    const { container } = render(
      <BrowserRouter>
        <App />
      </BrowserRouter>
    );
    expect(container).toBeTruthy();
  });

  it('contains V1 navigation items', () => {
    render(
      <BrowserRouter>
        <App />
      </BrowserRouter>
    );

    const navigation = screen.getByRole('navigation');
    expect(within(navigation).getByRole('link', { name: /dashboard/i })).toBeTruthy();
    expect(within(navigation).getByRole('link', { name: /carte \+ timeline/i })).toBeTruthy();
    expect(within(navigation).getByRole('link', { name: /eventcase/i })).toBeTruthy();
    expect(within(navigation).getByRole('link', { name: /observations/i })).toBeTruthy();
    expect(within(navigation).getByRole('link', { name: /paramètres/i })).toBeTruthy();
  });

  it('renders the Dashboard heading on the default route', () => {
    render(
      <BrowserRouter>
        <App />
      </BrowserRouter>
    );

    expect(screen.getByRole('heading', { name: /dashboard/i, level: 1 })).toBeTruthy();
  });
});