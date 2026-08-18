import { Component, Input, OnChanges, ViewChild, ViewContainerRef } from '@angular/core';
import { WidgetConfig } from '../widgets/widget.model';

/**
 * Renders a config array of widgets by dynamically creating each component
 * via ViewContainerRef.createComponent() - the widget list (which KPI cards,
 * in what order, with what data) is data, not a hardcoded template. Adding a
 * new widget type later never touches this component, only the config array
 * a page builds.
 */
@Component({
  selector: 'app-widget-host',
  template: `<div class="row g-3"><ng-container #container></ng-container></div>`,
})
export class WidgetHostComponent implements OnChanges {
  @Input({ required: true }) widgets: WidgetConfig[] = [];
  @ViewChild('container', { read: ViewContainerRef, static: true }) container!: ViewContainerRef;

  ngOnChanges(): void {
    this.container.clear();
    for (const widget of this.widgets) {
      const ref = this.container.createComponent(widget.component);
      for (const [key, value] of Object.entries(widget.inputs ?? {})) {
        ref.setInput(key, value);
      }
    }
  }
}
