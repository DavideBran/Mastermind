import { CommonModule } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-instruction',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './instruction.component.html',
  styleUrl: './instruction.component.css'
})
export class InstructionComponent {
  viewInstruction = false;
  @Input() darkMode = false;

  constructor(){}

}
