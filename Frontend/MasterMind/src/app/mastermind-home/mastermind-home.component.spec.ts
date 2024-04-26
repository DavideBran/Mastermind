import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MastermindHomeComponent } from './mastermind-home.component';

describe('MastermindHomeComponent', () => {
  let component: MastermindHomeComponent;
  let fixture: ComponentFixture<MastermindHomeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MastermindHomeComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(MastermindHomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
