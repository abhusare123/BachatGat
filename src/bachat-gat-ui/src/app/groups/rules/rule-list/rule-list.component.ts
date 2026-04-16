import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { GroupRulesService } from '../../../core/group-rules.service';
import { AuthService } from '../../../core/auth.service';
import { GroupService } from '../../../core/group.service';
import { ConfigurableRule, GroupRulesResponse, GroupMemberRole } from '../../../core/models';
import { EditRuleDialogComponent } from '../edit-rule-dialog/edit-rule-dialog.component';

interface FixedRule {
  id: number;
  category: 'Rules' | 'Conditions' | 'Guidelines';
  categoryMr: string;
  text: string;
  textMr: string;
}

@Component({
  selector: 'app-rule-list',
  imports: [
    CommonModule, MatExpansionModule, MatTableModule, MatButtonModule,
    MatIconModule, MatProgressSpinnerModule, MatDialogModule, MatTooltipModule,
    MatDividerModule
  ],
  templateUrl: './rule-list.component.html',
  styleUrl: './rule-list.component.scss'
})
export class RuleListComponent implements OnInit {
  groupId!: number;
  rulesData?: GroupRulesResponse;
  loading = true;
  isAdmin = false;

  readonly fixedRules: FixedRule[] = [
    // Rules
    { id: 1,  category: 'Rules', categoryMr: 'नियम', text: 'Female members of the Self-Help Group must not reside outside the village.', textMr: 'बचत गटाच्या महिला सदस्यांनी गावाबाहेर राहता कामा नये.' },
    { id: 2,  category: 'Rules', categoryMr: 'नियम', text: 'All members must contribute an equal amount of savings and attend group meetings regularly. No member is permitted to remain absent without prior notice.', textMr: 'सर्व सदस्यांनी समान बचत करावी आणि बैठकांना नियमित उपस्थित राहावे. पूर्वसूचनेशिवाय गैरहजर राहणे अनुज्ञेय नाही.' },
    { id: 3,  category: 'Rules', categoryMr: 'नियम', text: 'A penalty shall be imposed if the savings contribution is not deposited regularly.', textMr: 'नियमित बचत न केल्यास दंड आकारला जाईल.' },
    { id: 4,  category: 'Rules', categoryMr: 'नियम', text: 'For loan repayment, a specific timeframe and appropriate installment amounts shall be determined. The monthly interest rate must not exceed 3%.', textMr: 'कर्ज परताव्यासाठी विशिष्ट कालावधी व हप्त्याची रक्कम निश्चित केली जाईल. मासिक व्याजदर ३% पेक्षा जास्त असू नये.' },
    { id: 5,  category: 'Rules', categoryMr: 'नियम', text: 'Loans shall not be granted to individuals outside the group. However, loans may be granted to another Self-Help Group within the same village.', textMr: 'गटाबाहेरील व्यक्तीला कर्ज दिले जाणार नाही. मात्र, त्याच गावातील दुसऱ्या बचत गटाला कर्ज देण्यास हरकत नाही.' },
    { id: 6,  category: 'Rules', categoryMr: 'नियम', text: 'Men are not permitted to participate in a women\'s Self-Help Group, nor may they participate in decision-making or apply for loans on behalf of their wives.', textMr: 'पुरुषांना महिला बचत गटात सहभागी होता येणार नाही, तसेच निर्णय प्रक्रियेत भाग घेता येणार नाही किंवा पत्नीच्या नावे कर्जासाठी अर्ज करता येणार नाही.' },
    { id: 7,  category: 'Rules', categoryMr: 'नियम', text: 'If a loan amount exceeding the prescribed limit is required, two members of the Self-Help Group must stand as guarantors for the borrower.', textMr: 'निर्धारित मर्यादेपेक्षा जास्त कर्ज हवे असल्यास दोन सदस्यांनी जामीन द्यावा.' },
    { id: 8,  category: 'Rules', categoryMr: 'नियम', text: 'A second loan shall not be granted until the first loan has been fully repaid.', textMr: 'पहिले कर्ज पूर्णपणे फेडल्याशिवाय दुसरे कर्ज दिले जाणार नाही.' },
    { id: 9,  category: 'Rules', categoryMr: 'नियम', text: 'Loan funds must be utilized solely for the purpose for which they were sanctioned. A committee of three women shall monitor compliance.', textMr: 'मंजूर कारणासाठीच कर्जाचा वापर करावा. तीन महिलांची समिती याचे पालन होते का हे तपासेल.' },
    { id: 10, category: 'Rules', categoryMr: 'नियम', text: 'When the group intends to secure a loan from an external institution (e.g., a bank), it must obtain the approval of at least 75% of its members. This decision must be documented in writing.', textMr: 'बँकेसारख्या बाह्य संस्थेकडून कर्ज घेण्यापूर्वी किमान ७५% सदस्यांची लेखी मंजुरी घेणे आवश्यक आहे.' },
    // Conditions
    { id: 11, category: 'Conditions', categoryMr: 'अटी', text: 'A Self-Help Group must consist of a minimum of 10 and a maximum of 20 members.', textMr: 'बचत गटात किमान १० आणि कमाल २० सदस्य असावेत.' },
    { id: 12, category: 'Conditions', categoryMr: 'अटी', text: 'Each member must be over 18 years of age or be married.', textMr: 'प्रत्येक सदस्य १८ वर्षांपेक्षा जास्त वयाची किंवा विवाहित असावी.' },
    { id: 13, category: 'Conditions', categoryMr: 'अटी', text: 'If a woman wishes to join after the group\'s inception, she may be admitted with unanimous consent of all existing members, provided total membership stays within the prescribed limit. The new member must contribute the same funds as original members.', textMr: 'गट स्थापन झाल्यानंतर सहभागी होऊ इच्छिणाऱ्या महिलेला सर्व सदस्यांच्या एकमताने प्रवेश देता येईल, परंतु एकूण सदस्यसंख्या मर्यादेत राहिली पाहिजे. नवीन सदस्याने मूळ सदस्यांप्रमाणेच निधी भरावा.' },
    { id: 14, category: 'Conditions', categoryMr: 'अटी', text: 'Within six months of the group\'s formation, all younger members must be able to sign their names. Only elderly members may be permitted to use a thumb impression.', textMr: 'गट स्थापन झाल्यापासून सहा महिन्यांत तरुण सदस्यांना स्वाक्षरी करता आली पाहिजे. केवळ वृद्ध सदस्यांना अंगठा वापरण्याची परवानगी असेल.' },
    // Guidelines
    { id: 15, category: 'Guidelines', categoryMr: 'मार्गदर्शक तत्त्वे', text: 'During the first meeting, the group formation must be approved by all members. A President should be elected, and the group\'s rules, conditions, and guidelines should be read aloud and formally approved.', textMr: 'पहिल्या बैठकीत सर्व सदस्यांनी गट स्थापनेस मान्यता द्यावी. अध्यक्षाची निवड करावी आणि नियम, अटी व मार्गदर्शक तत्त्वे मोठ्याने वाचून मान्य करावीत.' },
    { id: 16, category: 'Guidelines', categoryMr: 'मार्गदर्शक तत्त्वे', text: 'The date, time, and venue for the monthly meetings should be finalized.', textMr: 'मासिक बैठकांची तारीख, वेळ आणि ठिकाण निश्चित करावे.' },
    { id: 17, category: 'Guidelines', categoryMr: 'मार्गदर्शक तत्त्वे', text: 'The group President and other members should learn how to manage the group\'s financial accounts and bookkeeping.', textMr: 'अध्यक्ष व इतर सदस्यांनी गटाचे आर्थिक हिशेब व लेखांकन व्यवस्थापन शिकावे.' },
    { id: 18, category: 'Guidelines', categoryMr: 'मार्गदर्शक तत्त्वे', text: 'All records—including the ledger, passbook, and meeting minutes register—must be updated immediately after the conclusion of each meeting.', textMr: 'खतावणी, पासबुक आणि इतिवृत्त नोंदवही प्रत्येक बैठकीनंतर लगेच अद्ययावत करावी.' },
    { id: 19, category: 'Guidelines', categoryMr: 'मार्गदर्शक तत्त्वे', text: 'The minutes of the previous meeting should be read aloud during the current meeting.', textMr: 'मागील बैठकीचे इतिवृत्त सध्याच्या बैठकीत मोठ्याने वाचावे.' },
    { id: 20, category: 'Guidelines', categoryMr: 'मार्गदर्शक तत्त्वे', text: 'The group must establish clear rules at the very outset regarding the priority assigned to different types of loan applications when disbursing funds.', textMr: 'निधी वितरणावेळी विविध प्रकारच्या कर्ज अर्जांना प्राधान्यक्रम निश्चित करण्याचे नियम सुरुवातीसच ठरवावेत.' },
    { id: 21, category: 'Guidelines', categoryMr: 'मार्गदर्शक तत्त्वे', text: 'Discussions during SHG meetings should extend to topics such as education, health, business, small-scale industries, and personal challenges.', textMr: 'बैठकींमध्ये शिक्षण, आरोग्य, व्यवसाय, लघुउद्योग, वैयक्तिक समस्या यांवरही चर्चा व्हावी.' },
    { id: 22, category: 'Guidelines', categoryMr: 'मार्गदर्शक तत्त्वे', text: 'Funds accumulated by the SHG may be utilized to initiate activities such as a nursery, kitchen garden, cottage industry, or other enterprises relevant to the local region.', textMr: 'गटाने जमवलेल्या निधीचा वापर रोपवाटिका, किचन गार्डन, कुटिरोद्योग किंवा स्थानिक व्यवसायांसाठी करता येईल.' },
    { id: 23, category: 'Guidelines', categoryMr: 'मार्गदर्शक तत्त्वे', text: 'The disbursement of loans must be decided upon unanimously.', textMr: 'कर्ज वितरणाचा निर्णय एकमताने घ्यावा.' },
    { id: 24, category: 'Guidelines', categoryMr: 'मार्गदर्शक तत्त्वे', text: 'Once loans have been disbursed, if other groups do not require the remaining funds, the amount should be deposited in the bank.', textMr: 'कर्ज दिल्यानंतर उर्वरित रक्कम इतर गटांना नको असल्यास बँकेत जमा करावी.' },
    { id: 25, category: 'Guidelines', categoryMr: 'मार्गदर्शक तत्त्वे', text: 'Self-Help Groups should have their account registers audited by a local expert at least once a year.', textMr: 'बचत गटांच्या खाते नोंदींचे स्थानिक तज्ञाकडून वर्षातून किमान एकदा लेखापरीक्षण करावे.' },
  ];

  readonly ruleCategories: { key: 'Rules' | 'Conditions' | 'Guidelines'; labelEn: string; labelMr: string; icon: string }[] = [
    { key: 'Rules',      labelEn: 'Rules',      labelMr: 'नियम',                  icon: 'gavel' },
    { key: 'Conditions', labelEn: 'Conditions', labelMr: 'अटी',                   icon: 'checklist' },
    { key: 'Guidelines', labelEn: 'Guidelines', labelMr: 'मार्गदर्शक तत्त्वे',    icon: 'menu_book' },
  ];

  rulesFor(category: 'Rules' | 'Conditions' | 'Guidelines'): FixedRule[] {
    return this.fixedRules.filter(r => r.category === category);
  }

  constructor(
    private route: ActivatedRoute,
    private rulesService: GroupRulesService,
    private groupService: GroupService,
    private authSvc: AuthService,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    let r = this.route.snapshot;
    while (r && !r.paramMap.has('id')) r = r.parent!;
    this.groupId = +(r?.paramMap.get('id') ?? 0);
    this.load();
  }

  load() {
    this.loading = true;
    this.rulesService.getRules(this.groupId).subscribe({
      next: data => {
        this.rulesData = data;
        this.loading = false;
        this.checkAdmin();
      },
      error: () => this.loading = false
    });
  }

  private checkAdmin() {
    this.groupService.getGroup(this.groupId).subscribe(g => {
      const userId = this.authSvc.currentUser()?.userId;
      if (userId && g.members) {
        this.isAdmin = g.members.some(m => m.userId === userId && m.role === GroupMemberRole.Admin);
      }
    });
  }

  editRule(rule: ConfigurableRule) {
    this.dialog.open(EditRuleDialogComponent, {
      data: { groupId: this.groupId, rule },
      width: '420px'
    }).afterClosed().subscribe(saved => { if (saved) this.load(); });
  }

  formatValue(rule: ConfigurableRule): string {
    const num = parseFloat(rule.value);
    if (rule.unit === 'Rs') return `₹${num.toLocaleString('en-IN')}`;
    if (rule.unit === '%') return `${num}%`;
    return rule.value;
  }
}
