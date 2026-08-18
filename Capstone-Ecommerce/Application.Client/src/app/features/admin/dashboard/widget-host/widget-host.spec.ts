import { Component, Input } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { WidgetHostComponent } from './widget-host';
import { WidgetConfig } from '../widgets/widget.model';

@Component({ selector: 'app-test-widget-a', template: `<div class="widget-a">A: {{ label }}</div>` })
class TestWidgetAComponent {
  @Input() label = '';
}

@Component({ selector: 'app-test-widget-b', template: `<div class="widget-b">B: {{ count }}</div>` })
class TestWidgetBComponent {
  @Input() count = 0;
}

/**
 * Proves WidgetHostComponent's actual mechanism - ViewContainerRef.createComponent()
 * driven by a plain config array - rather than just checking it renders
 * something. TestWidgetA/B are unrelated to the real dashboard widgets on
 * purpose: WidgetHostComponent must have zero compile-time knowledge of
 * which components it's asked to create.
 */
describe('WidgetHostComponent', () => {
  let fixture: ComponentFixture<WidgetHostComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [WidgetHostComponent] });
    fixture = TestBed.createComponent(WidgetHostComponent);
  });

  it('renders nothing when the widget config array is empty', () => {
    fixture.componentRef.setInput('widgets', []);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.widget-a, .widget-b').length).toBe(0);
  });

  it('dynamically creates one real component instance per config entry, with its inputs applied', () => {
    const widgets: WidgetConfig[] = [
      { component: TestWidgetAComponent, inputs: { label: 'Revenue' } },
      { component: TestWidgetBComponent, inputs: { count: 42 } },
    ];

    fixture.componentRef.setInput('widgets', widgets);
    fixture.detectChanges();

    const widgetA = fixture.nativeElement.querySelector('.widget-a');
    const widgetB = fixture.nativeElement.querySelector('.widget-b');
    expect(widgetA?.textContent).toContain('Revenue');
    expect(widgetB?.textContent).toContain('42');
  });

  it('clears previously created widgets when the config array changes', () => {
    fixture.componentRef.setInput('widgets', [{ component: TestWidgetAComponent, inputs: { label: 'First' } }]);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('.widget-a').length).toBe(1);

    fixture.componentRef.setInput('widgets', [{ component: TestWidgetBComponent, inputs: { count: 1 } }]);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.widget-a').length).toBe(0);
    expect(fixture.nativeElement.querySelectorAll('.widget-b').length).toBe(1);
  });

  it('creates a widget with no inputs specified at all without throwing', () => {
    expect(() => {
      fixture.componentRef.setInput('widgets', [{ component: TestWidgetAComponent }]);
      fixture.detectChanges();
    }).not.toThrow();

    expect(fixture.nativeElement.querySelectorAll('.widget-a').length).toBe(1);
  });
});
