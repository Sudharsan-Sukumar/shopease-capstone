import { Type } from '@angular/core';

/** One entry in a dashboard's widget config array - what WidgetHostComponent turns into a real component via ViewContainerRef.createComponent(). */
export interface WidgetConfig {
  component: Type<unknown>;
  inputs?: Record<string, unknown>;
}
